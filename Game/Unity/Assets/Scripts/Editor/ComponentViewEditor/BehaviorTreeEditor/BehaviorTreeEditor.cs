using System;
using System.Collections.Generic;
using ET.BehaviorTree;
using ET.BehaviorTree.View;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ET.BehaviorTreeEditor
{
    public class BehaviorTreeEditor : EditorWindow
    {
        [MenuItem("ET/行为树编辑器")]
        public static void Create()
        {
            CreateWindow<BehaviorTreeEditor>();
        }

        private RootNode Root;

        private TreeViewState treeViewState;

        private BehaviorTreeView treeView;

        private NodeTypeSelector NodeTypeSelector;
        
        public void OnGUI()
        {
            if (GUILayout.Button("创建一个树"))
            {
                this.Root = new RootNode();
                treeViewState = new();
                NodeTypeSelector = new NodeTypeSelector(new List<Type>() { typeof (Sequence), typeof (UnityDebugNode) });
                treeView = new BehaviorTreeView(treeViewState, this.Root,NodeTypeSelector);
            }

            if (this.Root == null || treeView== null)
                return;
            
            var lastRect = GUILayoutUtility.GetLastRect();

            var startY = lastRect.y + lastRect.height;
            var totalWidth = this.position.width;
            var totalHeight = this.position.height - startY;

            if (treeView.CurrDrawingNodePT == null)
            {
                var totalRect = new Rect(0,lastRect.y+lastRect.height,totalWidth,totalHeight);
                treeView.OnGUI(totalRect);
            }
            else
            {
                var leftRect = new Rect(0,startY,totalWidth * 0.6f,totalHeight);
                treeView.OnGUI(leftRect);

                var rightRect = new Rect(leftRect.x + leftRect.width, startY, totalWidth - leftRect.width, totalHeight);
                GUILayout.BeginArea(rightRect);
                treeView.CurrDrawingNodePT.BeginDraw(false);
                foreach (var v in treeView.CurrDrawingNodePT.EnumerateTree(true,true))
                {
                    if (!v.Path.Contains("Child"))
                    {
                        v.Draw();
                    }
                }
                treeView.CurrDrawingNodePT.EndDraw();
                GUILayout.EndArea();

            }

        }
    }
}