using System;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ET
{
    public class DataGameObject
    {
        public string Name;
        public Entity Entity;
        public List<DataGameObject> Childs = new();
    }

    public class GameDrawer : EditorWindow
    {
        [MenuItem("ET/数据层可视化")]
        public static void Create()
        {
            CreateWindow<GameDrawer>();
        }

        private DataGameObject Root;
        private Dictionary<int, bool> hashcode2Foldout = new();

        private Vector2 ScrollPoint = Vector2.zero;
        
        private void OnGUI()
        {
            if (GUILayout.Button("BuildAll"))
            {
                BuildAll();
            }

            if (this.Root != null)
            {
                ScrollPoint = EditorGUILayout.BeginScrollView(ScrollPoint);
                DrawAll(this.Root);
                EditorGUILayout.EndScrollView();
            }

        }


        void BuildAll()
        {
            ScrollPoint = Vector2.zero;
            hashcode2Foldout.Clear();
            var r = ET.Root.Instance.Scene;
            this.Root = new DataGameObject() { Name = "RootScene", Entity = r };
            Build(r,this.Root);
        }

        void Build(Entity parent,DataGameObject dataParent)
        {
            foreach (var v in parent.Components)
            {
                var data = new DataGameObject() { Name = v.Key.FullName, Entity = v.Value };
                dataParent.Childs.Add(data);
                Build(v.Value,data);
            }

            if (parent.Children.Count > 0)
            {
                var child = new DataGameObject() { Name = "[Childs]" };

                dataParent.Childs.Add(child);
                foreach (var v in parent.Children)
                {
                    var data = new DataGameObject() { Name = v.Value.GetType().Name, Entity = v.Value };
                    child.Childs.Add(data);
                    Build(v.Value, data);
                }
            }
        }



        void DrawAll(DataGameObject data)
        {
            if (data.Childs.Count > 0)
            {
                var dataHashCode = data.GetHashCode();
                if (!hashcode2Foldout.ContainsKey(dataHashCode))
                    hashcode2Foldout[dataHashCode] = true;

                hashcode2Foldout[dataHashCode] = EditorGUILayout.Foldout(hashcode2Foldout[dataHashCode], data.Name);
                if (hashcode2Foldout[dataHashCode])
                {
                    EditorGUI.indentLevel++;
                    foreach (var v in data.Childs)
                    {
                        DrawAll(v);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUILayout.LabelField(data.Name);
            }
        }



    }
}