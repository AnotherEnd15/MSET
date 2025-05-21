using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
//Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;

//自定义ReferenceCollector类在界面中的显示与功能
[CustomEditor(typeof (ReferenceCollector))]
public class ReferenceCollectorEditor: OdinEditor
{
    //输入在textfield中的字符串
    private string searchKey
	{
		get
		{
			return _searchKey;
		}
		set
		{
			if (_searchKey != value)
			{
				_searchKey = value;
				heroPrefab = referenceCollector.Get<Object>(searchKey);
			}
		}
	}

	private ReferenceCollector referenceCollector;

	private Object heroPrefab;

	private string _searchKey = "";

	private void DelNullReference()
	{
		var dataProperty = serializedObject.FindProperty("data");
		for (int i = dataProperty.arraySize - 1; i >= 0; i--)
		{
			var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
			if (gameObjectProperty.objectReferenceValue == null)
			{
				dataProperty.DeleteArrayElementAtIndex(i);
				EditorUtility.SetDirty(referenceCollector);
				serializedObject.ApplyModifiedProperties();
				serializedObject.UpdateIfRequiredOrScript();
			}
		}
	}

	private void OnEnable()
	{
        //将被选中的gameobject所挂载的ReferenceCollector赋值给编辑器类中的ReferenceCollector，方便操作
        referenceCollector = (ReferenceCollector) target;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		var eventType = Event.current.type;
        //在Inspector 窗口上创建区域，向区域拖拽资源对象，获取到拖拽到区域的对象
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
		{
			// Show a copy icon on the drag
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			if (eventType == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				foreach (var o in DragAndDrop.objectReferences)
				{
					AddReference(o.name, o);
				}
			}

			Event.current.Use();
		}
        
	}

    //添加元素，具体知识点在ReferenceCollector中说了
    private void AddReference(string key, Object obj)
	{
		(this.target as ReferenceCollector).Add(key,obj);
	}
}
