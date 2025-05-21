using ET;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace ETEditor
{
#if true
    public class EditorListHelper<T>
        where T : class
    {
        private List<T> objs;
        private List<PropertyTree> showTrees = new List<PropertyTree>();
        private List<bool> isFoldOuts = new List<bool>();

        private Vector2 scrollViewPos = Vector2.zero;

        private List<Rect> nodePos = new List<Rect>();

        float endY;
        Rect mask;

        T copyObj;
        float upHeight = 20;
        float leftWidth = 40;

        public void Init(List<T> objs)
        {
            this.objs = objs;
            showTrees.Clear();
            isFoldOuts.Clear();
            scrollViewPos = Vector2.zero;
            foreach (var obj in this.objs)
            {
                showTrees.Add(PropertyTree.Create(obj));
                this.isFoldOuts.Add(true);
            }
        }
        public string GetTypeName(Type type)
        {
            var attribute = type.GetAttribute<LabelTextAttribute>();
            return attribute == null ? type.Name : attribute.Text;
        }
        public void Draw(Rect mask)
        {
            this.mask = mask;
            if (Event.current.type == EventType.Repaint)
            {
                endY = GUILayoutUtility.GetLastRect().yMax + 2;
            }
            UserInput(Event.current);
            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos, false, false);

            for (int i = 0; i < objs.Count; i++)
            {
                var pos=SirenixEditorGUI.BeginBox();
                if (Event.current.type == EventType.Repaint)
                {
                    if (nodePos.Count <= i) nodePos.Add(pos);
                    else nodePos[i] = pos;
                }
                if (!DrawNode(i))
                {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    return;
                }
                SirenixEditorGUI.EndBox();
            }
            EditorGUILayout.EndScrollView();
        }

        private bool DrawNode(int i)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(upHeight));
            isFoldOuts[i] = EditorGUILayout.Foldout(isFoldOuts[i], GetTypeName(objs[i].GetType()) + ":");
            if (GUILayout.Button("-", GUILayout.Width(100)))
            {
                objs.RemoveAt(i);
                showTrees.RemoveAt(i);
                isFoldOuts.RemoveAt(i);
                GUILayout.EndHorizontal();
                return false;
            }
            GUILayout.EndHorizontal();
            if (isFoldOuts[i])
            {
                Rect rect = EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical(GUILayout.Width(leftWidth));
                if (i > 0)
                {
                    if (GUILayout.Button("^", GUILayout.Height(upHeight)))
                    {
                        Exchange(i, i - 1);
                    }
                }
                else
                {
                    EditorGUILayout.Space(upHeight);
                }
                if (i < objs.Count - 1)
                {
                    if (GUILayout.Button("v", GUILayout.Height(upHeight)))
                    {
                        Exchange(i, i + 1);
                    }
                }
                else
                {
                    EditorGUILayout.Space(upHeight);
                }

                EditorGUILayout.EndVertical();
                GUILayout.BeginVertical();
                showTrees[i].Draw(false);
                GUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            return true;
        }

        void Exchange(int oldIndex, int newIndex)
        {
            void ExchangeListItem(IList list)
            {
                var old = list[oldIndex];
                list[oldIndex] = list[newIndex];
                list[newIndex] = old;
            }
            ExchangeListItem(objs);
            ExchangeListItem(showTrees);
            ExchangeListItem(isFoldOuts);
        }
        void UserInput(Event e)
        {
            if (e.type == EventType.MouseUp)
            {
                if (e.button == 1)
                {
                    Vector2 mousePos = new Vector2(e.mousePosition.x, e.mousePosition.y - endY + scrollViewPos.y);
                    for (int i = 0; i < nodePos.Count; i++)
                    {
                        Rect pos = nodePos[i];

                        if (pos.Contains(mousePos)
                            && !new Rect(pos.x + leftWidth, pos.y + upHeight
                            , pos.width - leftWidth, pos.height - upHeight).Contains(mousePos))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent() { text = "复制" + GetTypeName(objs[i].GetType()) }, false,
                                () => copyObj = objs[i]);
                            if (copyObj != null && copyObj != objs[i]
                                && copyObj.GetType() == objs[i].GetType())
                            {
                                menu.AddItem(new GUIContent() { text = "粘贴" + GetTypeName(objs[i].GetType()) }, false,
                                    () =>
                                    {
                                        objs[i] = DepthCopy(copyObj);
                                        showTrees[i] = PropertyTree.Create(objs[i]);
                                        });
                            }
                            menu.ShowAsContext();
                            break;
                        }
                    }
                }
            }
        }

        public static K DepthCopy<K>(K source)
            where K:class
        {
            if (source == null) return null;
            MethodInfo Deserialize = typeof(MemoryPackHelper)
                .GetMethod("Deserialize", new[] { typeof(Type), typeof(byte[]),typeof(int),typeof(int)});

            byte[] serialize= MemoryPackHelper.Serialize(source);
            return Deserialize.Invoke(null, new object[] {source.GetType(),serialize,0,serialize.Length }) as K;
        }

    }
#endif

#if false
public class EditorListHelper<T>
    where T:class
{
    private List<T> objs;
    private List<PropertyTree> showTrees = new List<PropertyTree>();
    private List<bool> isFoldOuts= new List<bool>();

    private Vector2 scrollViewPos = Vector2.zero;
    private List<Rect> posInfos= new List<Rect>();
    private Vector2 dragStartPos;
    private float dragY;
    private int dragIndex=-1;

    bool isRepait = false;
    float endY;
    Rect mask;
    bool exChange;

    public void Init(List<T> objs)
    {
        this.objs = objs;
        showTrees.Clear();
        isFoldOuts.Clear();
        scrollViewPos = Vector2.zero;
        foreach (var obj in this.objs)
        {
            showTrees.Add(PropertyTree.Create(obj));
            this.isFoldOuts.Add(true);
            posInfos.Add(new Rect());
        }
    }
    public string GetTypeName(Type type)
    {
        var attribute = type.GetAttribute<LabelTextAttribute>();
        return attribute == null ? type.Name : attribute.Text;
    }
    public void ChangePosInfo(int index,Rect rect)
    {
        if(posInfos.Count<=index)
        {
            posInfos.Add(new Rect());
        }
        if (Event.current.type == EventType.Repaint)
        {
            exChange = false;
            if (index == dragIndex)
            {
                posInfos[index]=new Rect(rect.x, rect.y, rect.width, posInfos[index].height);
            }
            else
            {
                posInfos[index] = rect;
            }
        }
    }

    public void Draw(Rect mask)
    {
        this.mask = mask;
        if (Event.current.type == EventType.Repaint)
        {
            endY = GUILayoutUtility.GetLastRect().yMax+2;
        }
        UserInput(Event.current);
        bool showDragWindow =false;
        scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos, false, false);

        for (int i = 0; i < objs.Count; i++)
        {
            var rect = SirenixEditorGUI.BeginBox();
            ChangePosInfo(i, rect);
            if (i == dragIndex)
            {
                SirenixEditorGUI.BeginBox();
                GUILayout.Space(posInfos[i].height-6);
                SirenixEditorGUI.EndBox();
                showDragWindow = true;
            }
            else
            {
                if (!DrawNode(i))
                {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    return;
                }
            }
            SirenixEditorGUI.EndBox();
            GUILayout.Space(20);
        }
        EditorGUILayout.EndScrollView();

        if(showDragWindow)
        {
            Rect nodeRect = new Rect(0, dragY+endY+2, mask.width, posInfos[dragIndex].height);
            GUILayout.BeginArea(nodeRect);
            DrawNode(dragIndex);
            GUILayout.EndArea();
        }
    }

    private bool DrawNode(int i)
    {
        EditorGUILayout.BeginHorizontal();
        isFoldOuts[i] = EditorGUILayout.Foldout(isFoldOuts[i], GetTypeName(objs[i].GetType()) + ":");
        if (GUILayout.Button("-", GUILayout.Width(100)))
        {
            objs.RemoveAt(i);
            showTrees.RemoveAt(i);
            isFoldOuts.RemoveAt(i);
            posInfos.RemoveAt(i);
            GUILayout.EndHorizontal();
            return false;
        }
        GUILayout.EndHorizontal();
        if (isFoldOuts[i])
        {
            Rect rect=EditorGUILayout.BeginHorizontal();

            SirenixEditorGUI.BeginBox(GUILayout.Width(50));
            GUILayout.Label("《=》", GUILayout.Width(50), GUILayout.Height(posInfos[i].height*0.7f));
            SirenixEditorGUI.EndBox();

            GUILayout.BeginVertical();
            showTrees[i].Draw(false);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        return true;
    }

    public int ContainClickIndex(Vector2 pos)
    {
        float scrollY = -scrollViewPos.y;
        for (int i = 0; i < posInfos.Count; i++)
        {
            Rect item = posInfos[i];
            if (item.yMin+20+ scrollY < pos.y&&item.yMax+ scrollY > pos.y
                &&item.xMin<pos.x&&item.xMin+50>pos.x)
            {
                return i;
            }
        }
        return -1;
    }
    public int ContainExchangeIndex(float posY)
    {
        float scrollY = -scrollViewPos.y;
        if (posInfos[dragIndex].yMin+ scrollY < posY && posInfos[dragIndex].yMax+ scrollY > posY)
        {
            return -1;
        }

        int exchangeIndex = -1;
        for (int i = 0; i < objs.Count; i++)
        {
            Rect item = posInfos[i];
            if(i>dragIndex)
            {
                if (item.yMax + scrollY < posY)
                {
                    exchangeIndex = i;
                }
            }
            else if(i<dragIndex)
            {
                if (item.yMin + scrollY > posY)
                {
                    exchangeIndex = i;
                }
            }
        }
        return exchangeIndex;
    }

    void UserInput(Event e)
    {
        if (e.type == EventType.MouseDown)
        {
            dragIndex = ContainClickIndex(e.mousePosition-Vector2.up*endY);
            if (dragIndex >= 0)
            {
                dragY = posInfos[dragIndex].y - scrollViewPos.y;
                dragStartPos = e.mousePosition;
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            dragY = 0;
            exChange = false;
            dragIndex = -1;
        }

        if (dragIndex>=0&&e.type == EventType.MouseDrag&&!exChange)
        {
            dragY += (e.mousePosition - dragStartPos).y;
            dragStartPos = e.mousePosition;
            var newIndex = ContainExchangeIndex(e.mousePosition.y - endY);
            if(newIndex>=0&&newIndex!=dragIndex)
            {
                exChange = true;
                void ExchangeListItem(IList list,int oldIndex,int newIndex)
                {
                    var old = list[oldIndex];
                    list[oldIndex] = list[newIndex];
                    list[newIndex] = old;
                }
                ExchangeListItem(posInfos,dragIndex,newIndex);
                ExchangeListItem(objs, dragIndex, newIndex);
                ExchangeListItem(showTrees, dragIndex, newIndex);
                ExchangeListItem(isFoldOuts, dragIndex, newIndex);
                dragIndex=newIndex;
            }
            else if (e.mousePosition.y>mask.y+mask.height)
            {
                scrollViewPos.y+=10;
            }
            else if(e.mousePosition.y<endY)
            {
                scrollViewPos.y -= 10;
            }
        }
    }
}
#endif
}