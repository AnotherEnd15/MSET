using System;
using System.Collections.Generic;
using System.Linq;
using ET.BehaviorTree;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ET.BehaviorTreeEditor
{
    public class BehaviorTreeView : TreeView
    {
        public RootNode RootNode;
        public NodeTypeSelector NodeTypeSelector;
        public Dictionary<int, BaseNode> Id2Nodes;
        public Dictionary<BaseNode, int> Node2Ids;

        public Dictionary<BaseNode, PropertyTree> PropertyTrees = new();

        public PropertyTree CurrDrawingNodePT;

        public BehaviorTreeView(TreeViewState state,RootNode rootNode,NodeTypeSelector nodeTypeSelector): base(state)
        {
            this.RootNode = rootNode;
            this.NodeTypeSelector = nodeTypeSelector;
            this.NodeTypeSelector.SelectionConfirmed += NodeTypeSelector_OnSelectionConfirmed;
            
            PropertyTrees[RootNode] = PropertyTree.Create(RootNode);
            
            this.Reload();
        }

        private void NodeTypeSelector_OnSelectionConfirmed(IEnumerable<Type> obj)
        {
            var first = obj.First();
            if (this.clickNode == null)
                return;

            var newObj = Activator.CreateInstance(first) as BaseNode;
            if (!PropertyTrees.ContainsKey(newObj))
            {
                PropertyTrees[newObj] = PropertyTree.Create(newObj);
            }
            this.clickNode.Childs.Add(newObj);
            newObj.Parent = this.clickNode;

            var select = this.clickNode;
            this.Reload();
            this.SetExpanded(this.Node2Ids[select], true);

        }

        public BehaviorTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader): base(state, multiColumnHeader)
        {
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }
        

        private int id;
        protected override TreeViewItem BuildRoot()
        {
            id = 0;
            clickNode = null;
            this.Id2Nodes = new();
            this.Node2Ids = new();
            var root = new TreeViewItem() { id = this.id++, depth = -1, displayName = "Root" };

            var start = new TreeViewItem() { id = this.id++, depth = 0, displayName = "Start" };
            this.Id2Nodes[start.id] = RootNode;
            this.Node2Ids[this.RootNode] = start.id;
            
            root.AddChild(start);
            
            BuildAll(start, RootNode);
            return root;
        }

        void BuildAll(TreeViewItem parent, CompNode compNode)
        {
            parent.children = new();
            foreach (var v in compNode.Childs)
            {
                var _id = this.id++;
                this.Id2Nodes[_id] = v;
                this.Node2Ids[v] = _id;
                var item = new TreeViewItem()
                {
                    id = _id,
                    depth = parent.depth+1,
                };
                BuildItemName(item);
                parent.AddChild(item);
                if (v is CompNode childComp)
                {
                    BuildAll(item, childComp);
                }
            }
        }
        
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0)
            {
                CurrDrawingNodePT = null;
                return;
            }

            var currSelected = this.Id2Nodes[selectedIds.First()];

            CurrDrawingNodePT = PropertyTrees[currSelected];
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;
            BuildItemName(item);
            base.RowGUI(args);
        }


        void BuildItemName(TreeViewItem item)
        {
            var node = this.Id2Nodes[item.id];
            if (node is CompNode compNode)
            {
                item.displayName = $"[{item.id}] [组合节点] {node.GetType().Name} {node.Comment}";   
            }
            else
            {
                item.displayName = $"[{item.id}] {node.GetType().Name} {node.Comment}";   
            }
        }


        protected override bool CanBeParent(TreeViewItem item)
        {
            var node = this.Id2Nodes[item.id];
            if (node is CompNode compNode)
                return true;
            return false;
        }

        private CompNode clickNode;

        protected override void ContextClickedItem(int id)
        {
            var node = this.Id2Nodes[id];
            if (node is not CompNode compNode)
            {
                clickNode = null;
                return;
            }
            
            clickNode = compNode;
            
            this.NodeTypeSelector.ShowInPopup();
        }
        

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        private int beginDragId;
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (args.draggedItemIDs.Count == 0)
            {
                beginDragId = -1;
                return;
            }
            
            DragAndDrop.PrepareStartDrag();
            beginDragId = args.draggedItemIDs.First();
            var beginNode = this.Id2Nodes[this.beginDragId];
            DragAndDrop.StartDrag($"[{beginDragId}] {beginNode.GetType().Name} {beginNode.Comment}");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (this.beginDragId < 0)
                return DragAndDropVisualMode.Rejected;


            if (!args.performDrop)
            {
                return DragAndDropVisualMode.Move;
            }


            DragAndDrop.AcceptDrag();
            
            var beginDragNode = this.Id2Nodes[this.beginDragId];
            this.beginDragId = -1;
            

            BaseNode targetNode = null;
            CompNode targetCompNode = null;
            int index = -1;
            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                {
                     targetNode = this.Id2Nodes[args.parentItem.id];
                     targetCompNode = targetNode as CompNode;
                     if (targetCompNode == null)
                     {
                         targetCompNode = targetNode.Parent;
                         index = targetCompNode.Childs.IndexOf(targetNode);
                     }
                     else
                     {
                         index = targetCompNode.Childs.Count;
                     }
                }
                    break;
                case DragAndDropPosition.BetweenItems:
                {
                    targetNode = this.Id2Nodes[args.parentItem.id];
                    targetCompNode = targetNode as CompNode;
                    index = args.insertAtIndex;
                }
                    break;
                case DragAndDropPosition.OutsideItems:
                    return DragAndDropVisualMode.Rejected;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (beginDragNode is CompNode compNode)
            {
                if (SrcIsChild(compNode, targetNode))
                {
                    return DragAndDropVisualMode.Rejected;
                }   
            }

            if (beginDragNode.Parent == targetCompNode)
            {
                if (index >= targetCompNode.Childs.Count)
                {
                    beginDragNode.Parent.Childs.Remove(beginDragNode);
                    targetCompNode.Childs.Add(beginDragNode);
                }
                else if (index <=0)
                {
                    targetCompNode.Childs.Remove(beginDragNode);
                    targetCompNode.Childs.Insert(0,beginDragNode);
                }
                else
                {
                    if (index == targetCompNode.Childs.Count -1)
                        index = targetCompNode.Childs.Count - 2;
                    
                    var tar = targetCompNode.Childs[index];
                    targetCompNode.Childs.Remove(beginDragNode);
                    var targetIndex = targetCompNode.Childs.IndexOf(tar);
                    targetCompNode.Childs.Insert(targetIndex,beginDragNode);
                }
            }
            else
            {
                beginDragNode.Parent.Childs.Remove(beginDragNode);
                if (index >= targetCompNode.Childs.Count)
                {
                    targetCompNode.Childs.Add(beginDragNode);
                }
                else
                {
                    targetCompNode.Childs.Insert(index,beginDragNode);   
                }
                beginDragNode.Parent = targetCompNode;   
            }

            var select = targetCompNode;
            this.Reload();
            this.SetExpanded(this.Node2Ids[select], true);
            
            return DragAndDropVisualMode.None;
        }

        bool SrcIsChild(CompNode srcNode, BaseNode targetNode)
        {
            foreach (var v in srcNode.Childs)
            {
                if (v is CompNode compNode)
                {
                    if (SrcIsChild(compNode, targetNode))
                    {
                        return true;
                    }
                }

                if (v == targetNode)
                {
                    return true;
                }
            }

            return false;
        }
    }
}