using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using UnityEngine.U2D.IK;
using Object = UnityEngine.Object;

namespace ET
{
    [Preserve]
    public class StripCode
    {
	    [Preserve]
	    public static void Call()
	    {
		    var text = new UnityWebRequest().SendWebRequest().webRequest.downloadHandler.text;
		    
		    Object.Instantiate(new GameObject(), null);
		    Object.Instantiate(new GameObject());
		    Object.Instantiate(new GameObject(), Vector3.one, Quaternion.identity);

		    var com = new GameObject().AddComponent<TrailRenderer>();
		    List<Type> list = new();
		    
		    list.Add(typeof(IKManager2D));
		    list.Add(typeof(ET.DoubleMap<object,ushort>));

		    list.Add(typeof(ET.ETAsyncTaskMethodBuilder<System.ValueTuple<object,int>>));
		    list.Add(typeof(ET.ETAsyncTaskMethodBuilder<byte>));
		    list.Add(typeof(ET.ETAsyncTaskMethodBuilder<int>));
		    list.Add(typeof(ET.ETAsyncTaskMethodBuilder<object>));
		    list.Add(typeof(ET.ETTask<System.ValueTuple<object,int>>));
		    list.Add(typeof(ET.ETTask<byte>));
		    list.Add(typeof(ET.ETTask<int>));
		    list.Add(typeof(ET.ETTask<object>));
		    list.Add(typeof(ET.HashSetComponent<int>));
		    list.Add(typeof(ET.HashSetComponent<long>));
		    list.Add(typeof(ET.HashSetComponent<object>));
		    list.Add(typeof(ET.ListComponent<int>));
		    list.Add(typeof(ET.ListComponent<object>));
		    list.Add(typeof(ET.MultiMap<long,long>));
		    list.Add(typeof(ET.MultiMap<object,FixMath.fp2>));
		    list.Add(typeof(ET.UnOrderMultiMap<int,int>));
		    list.Add(typeof(ET.UnOrderMultiMap<object,object>));
		    list.Add(typeof(ET.UnOrderMultiMapSet<int,object>));
		    list.Add(typeof(ET.UnOrderMultiMapSet<object,object>));
		    list.Add(typeof(LitJson.ExporterFunc<FixMath.fp2>));
		    list.Add(typeof(LitJson.ExporterFunc<FixMath.fp>));
		    list.Add(typeof(MemoryPack.Formatters.ArrayFormatter<byte>));
		    list.Add(typeof(MemoryPack.Formatters.ArrayFormatter<object>));
		    list.Add(typeof(MemoryPack.Formatters.DictionaryFormatter<int,FixMath.fp2>));
		    list.Add(typeof(MemoryPack.Formatters.DictionaryFormatter<int,int>));
		    list.Add(typeof(MemoryPack.Formatters.DictionaryFormatter<int,long>));
		    list.Add(typeof(MemoryPack.Formatters.DictionaryFormatter<int,object>));
		    list.Add(typeof(MemoryPack.Formatters.DictionaryFormatter<object,object>));
		    list.Add(typeof(MemoryPack.Formatters.ListFormatter<int>));
		    list.Add(typeof(MemoryPack.Formatters.ListFormatter<long>));
		    list.Add(typeof(MemoryPack.Formatters.ListFormatter<object>));
		    list.Add(typeof(MemoryPack.Formatters.UnmanagedFormatter<int>));
		    list.Add(typeof(MemoryPack.IMemoryPackFormatter<int>));
		    list.Add(typeof(MemoryPack.IMemoryPackFormatter<long>));
		    list.Add(typeof(MemoryPack.IMemoryPackFormatter<object>));
		    list.Add(typeof(MemoryPack.IMemoryPackable<object>));
		    list.Add(typeof(MemoryPack.MemoryPackFormatter<System.UIntPtr>));
		    list.Add(typeof(MemoryPack.MemoryPackFormatter<int>));
		    list.Add(typeof(MemoryPack.MemoryPackFormatter<object>));
		    list.Add(typeof(SerializableDictionary<int,object>));
		    list.Add(typeof(SerializableDictionaryBase<int,object,object>));
		    list.Add(typeof(SerializableDictionaryBase<object,object,object>));
		    list.Add(typeof(System.Action<FixMath.fp2>));
		    list.Add(typeof(System.Action<System.Collections.Generic.KeyValuePair<long,object>>));
		    list.Add(typeof(System.Action<System.Collections.Generic.KeyValuePair<object,object>>));
		    list.Add(typeof(System.Action<System.ValueTuple<int,int>>));
		    list.Add(typeof(System.Action<byte>));
		    list.Add(typeof(System.Action<float>));
		    list.Add(typeof(System.Action<int>));
		    list.Add(typeof(System.Action<long,int>));
		    list.Add(typeof(System.Action<long,long>));
		    list.Add(typeof(System.Action<long,object>));
		    list.Add(typeof(System.Action<long>));
		    list.Add(typeof(System.Action<object,ushort>));
		    list.Add(typeof(System.Action<object>));
		    list.Add(typeof(System.Action<ushort>));
		    list.Add(typeof(System.Collections.Concurrent.ConcurrentDictionary<object,object>));
		    list.Add(typeof(System.Collections.Generic.Comparer<FixMath.fp2>));

		    list.Add(typeof(System.Collections.Generic.Comparer<float>));
		    list.Add(typeof(System.Collections.Generic.Comparer<int>));
		    list.Add(typeof(System.Collections.Generic.Comparer<long>));
		    list.Add(typeof(System.Collections.Generic.Comparer<object>));
		    list.Add(typeof(System.Collections.Generic.Comparer<ushort>));
		 

		    list.Add(typeof(System.Collections.Generic.Dictionary<int,FixMath.fp2>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<int,int>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<int,long>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<int,object>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<long,FixMath.fp2>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<long,object>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<object,FixMath.fp2>));

		    list.Add(typeof(System.Collections.Generic.Dictionary<object,int>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<object,long>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<object,object>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<object,ushort>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<ushort,long>));
		    list.Add(typeof(System.Collections.Generic.Dictionary<ushort,object>));
		   
		    list.Add(typeof(System.Collections.Generic.EqualityComparer<FixMath.fp2>));

		    list.Add(typeof(System.Collections.Generic.EqualityComparer<int>));
		    list.Add(typeof(System.Collections.Generic.EqualityComparer<long>));
		    list.Add(typeof(System.Collections.Generic.EqualityComparer<object>));
		    list.Add(typeof(System.Collections.Generic.EqualityComparer<ushort>));

		    list.Add(typeof(System.Collections.Generic.HashSet<int>));
		    list.Add(typeof(System.Collections.Generic.HashSet<long>));
		    list.Add(typeof(System.Collections.Generic.HashSet<object>));
		    list.Add(typeof(System.Collections.Generic.HashSet<ushort>));
		    
		    list.Add(typeof(System.Collections.Generic.ICollection<FixMath.fp2>));
		 
		    list.Add(typeof(System.Collections.Generic.ICollection<float>));
		    list.Add(typeof(System.Collections.Generic.ICollection<int>));
		    list.Add(typeof(System.Collections.Generic.ICollection<long>));
		    list.Add(typeof(System.Collections.Generic.ICollection<object>));
		    list.Add(typeof(System.Collections.Generic.ICollection<ushort>));
		    list.Add(typeof(System.Collections.Generic.IComparer<FixMath.fp2>));
		
		    list.Add(typeof(System.Collections.Generic.IComparer<float>));
		    list.Add(typeof(System.Collections.Generic.IComparer<int>));
		    list.Add(typeof(System.Collections.Generic.IComparer<long>));
		    list.Add(typeof(System.Collections.Generic.IComparer<object>));
		    list.Add(typeof(System.Collections.Generic.IComparer<ushort>));
		    list.Add(typeof(System.Collections.Generic.IDictionary<int,object>));
		 
		    list.Add(typeof(System.Collections.Generic.IDictionary<object,object>));
		
		    list.Add(typeof(System.Collections.Generic.IEnumerable<FixMath.fp2>));
		
		    list.Add(typeof(System.Collections.Generic.IEnumerable<float>));
		    list.Add(typeof(System.Collections.Generic.IEnumerable<int>));
		    list.Add(typeof(System.Collections.Generic.IEnumerable<long>));
		    list.Add(typeof(System.Collections.Generic.IEnumerable<object>));
		    list.Add(typeof(System.Collections.Generic.IEnumerable<ushort>));
		  
		    list.Add(typeof(System.Collections.Generic.IEnumerator<float>));
		    list.Add(typeof(System.Collections.Generic.IEnumerator<int>));
		    list.Add(typeof(System.Collections.Generic.IEnumerator<long>));
		    list.Add(typeof(System.Collections.Generic.IEnumerator<object>));
		    list.Add(typeof(System.Collections.Generic.IEnumerator<ushort>));
		
		    list.Add(typeof(System.Collections.Generic.IEqualityComparer<int>));
		    list.Add(typeof(System.Collections.Generic.IEqualityComparer<long>));
		    list.Add(typeof(System.Collections.Generic.IEqualityComparer<object>));
		    list.Add(typeof(System.Collections.Generic.IEqualityComparer<ushort>));
		    list.Add(typeof(System.Collections.Generic.IList<FixMath.fp2>));
		  
		    list.Add(typeof(System.Collections.Generic.IList<float>));
		    list.Add(typeof(System.Collections.Generic.IList<int>));
		    list.Add(typeof(System.Collections.Generic.IList<long>));
		    list.Add(typeof(System.Collections.Generic.IList<object>));
		    list.Add(typeof(System.Collections.Generic.IList<ushort>));
		  
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<int,FixMath.fp2>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<int,int>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<int,long>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<int,object>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<long,FixMath.fp2>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<long,object>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<object,FixMath.fp2>));
		
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<object,int>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<object,long>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<object,object>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<object,ushort>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<ushort,long>));
		    list.Add(typeof(System.Collections.Generic.KeyValuePair<ushort,object>));
		   
		    list.Add(typeof(System.Collections.Generic.List<float>));
		    list.Add(typeof(System.Collections.Generic.List<int>));
		    list.Add(typeof(System.Collections.Generic.List<long>));
		    list.Add(typeof(System.Collections.Generic.List<object>));
		    list.Add(typeof(System.Collections.Generic.List<ushort>));
		
		  
		    list.Add(typeof(System.Collections.Generic.Queue<int>));
		    list.Add(typeof(System.Collections.Generic.Queue<long>));
		    list.Add(typeof(System.Collections.Generic.Queue<object>));
		 
		    list.Add(typeof(System.Collections.Generic.SortedDictionary<long,object>));
		    list.Add(typeof(System.Collections.Generic.SortedDictionary<object,object>));
		
		    list.Add(typeof(System.Collections.Generic.Stack<object>));
		
		    list.Add(typeof(System.Collections.ObjectModel.ReadOnlyCollection<FixMath.fp2>));
		  
		    list.Add(typeof(System.Collections.ObjectModel.ReadOnlyCollection<float>));
		    list.Add(typeof(System.Collections.ObjectModel.ReadOnlyCollection<int>));
		    list.Add(typeof(System.Collections.ObjectModel.ReadOnlyCollection<long>));
		    list.Add(typeof(System.Collections.ObjectModel.ReadOnlyCollection<object>));
		    list.Add(typeof(System.Collections.ObjectModel.ReadOnlyCollection<ushort>));
		    list.Add(typeof(System.Comparison<FixMath.fp2>));
		
		    list.Add(typeof(System.Comparison<float>));
		    list.Add(typeof(System.Comparison<int>));
		    list.Add(typeof(System.Comparison<long>));
		    list.Add(typeof(System.Comparison<object>));
		    list.Add(typeof(System.Comparison<ushort>));
		    list.Add(typeof(System.Converter<int,object>));
		    list.Add(typeof(System.Converter<object,int>));
		    list.Add(typeof(System.EventHandler<object>));
		
		    list.Add(typeof(System.Func<byte>));
		    list.Add(typeof(System.Func<object,byte>));
		    list.Add(typeof(System.Func<object,int>));
		    list.Add(typeof(System.Func<object,object>));
		
		    list.Add(typeof(System.Nullable<System.DateTimeOffset>));
		    list.Add(typeof(System.Nullable<double>));
		    list.Add(typeof(System.Nullable<int>));
		    list.Add(typeof(System.Predicate<FixMath.fp2>));
		
		    list.Add(typeof(System.Predicate<float>));
		    list.Add(typeof(System.Predicate<int>));
		    list.Add(typeof(System.Predicate<long>));
		    list.Add(typeof(System.Predicate<object>));
		    list.Add(typeof(System.Predicate<ushort>));
		    list.Add(typeof(System.ReadOnlySpan<byte>));
		    list.Add(typeof(System.ReadOnlySpan<object>));
		    list.Add(typeof(System.Span<byte>));
		    list.Add(typeof(System.Span<object>));
		    list.Add(typeof(System.Tuple<int,int,object>));
		    list.Add(typeof(System.ValueTuple<int,int>));
		    list.Add(typeof(System.ValueTuple<int,long,int>));
		    list.Add(typeof(System.ValueTuple<int,long>));
		    list.Add(typeof(System.ValueTuple<long,int>));
		    list.Add(typeof(System.ValueTuple<object,int>));
		    list.Add(typeof(System.ValueTuple<object,object>));
		    list.Add(typeof(System.ValueTuple<ushort,object>));
	    }
    }
}