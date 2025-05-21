using System;
using System.Collections.Generic;
using ET.BehaviorTree;
using Sirenix.OdinInspector.Editor;

namespace ET.BehaviorTreeEditor
{
    public class NodeTypeSelector : OdinSelector<Type>
    {
        private List<Type> srcs;
        
        public NodeTypeSelector(List<Type> types)
        {
            this.srcs = types;
        }
        
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;
            tree.Selection.SupportsMultiSelect = false;

            foreach (var v in this.srcs)
            {
                tree.Add(v.Name,v);
            }
        }
    }
}