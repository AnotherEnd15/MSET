using System;
using System.Collections.Generic;
using UnityEngine;
//Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;

//继承ISerializationCallbackReceiver后会增加OnAfterDeserialize和OnBeforeSerialize两个回调函数，如果有需要可以在对需要序列化的东西进行操作
//ET在这里主要是在OnAfterDeserialize回调函数中将data中存储的ReferenceCollectorData转换为dict中的Object，方便之后的使用
//注意UNITY_EDITOR宏定义，在编译以后，部分编辑器相关函数并不存在
public class ReferenceCollector: MonoBehaviour
{
	//Object并非C#基础中的Object，而是 UnityEngine.Object
	[SerializeField]
    private SerializableDictionary<string, Object> dict = new SerializableDictionary<string, Object>();

#if UNITY_EDITOR
    //添加新的元素
	public void Add(string key, Object obj)
	{
		dict[key] = obj;
	}
    //删除元素，知识点与上面的添加相似
	public void Remove(string key)
	{
		dict.Remove(key);
	}

	public void Clear()
	{
		this.dict.Clear();
	}
	
#endif
    //使用泛型返回对应key的gameobject
	public T Get<T>(string key) where T : class
	{
		Object dictGo;
		if (!dict.TryGetValue(key, out dictGo))
		{
			return null;
		}
		return dictGo as T;
	}

	public Object GetObject(string key)
	{
		Object dictGo;
		if (!dict.TryGetValue(key, out dictGo))
		{
			return null;
		}
		return dictGo;
	}
}
