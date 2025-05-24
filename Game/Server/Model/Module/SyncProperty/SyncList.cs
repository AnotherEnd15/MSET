using System;
using System.Collections;
using System.Collections.Generic;

namespace SyncFramework
{
	// 自动追踪变更的同步列表
	public class SyncList<T> : IList<T>, ISyncCollection
	{
		private List<T> _list = new List<T>();
		private Action<int, CollectionChange> _onChanged;
		private int _fieldId;

		public SyncList()
		{
		}

		// 实现ISyncCollection接口的SetChangeCallback方法
		public void SetChangeCallback(int fieldId, Action<int, CollectionChange> onChanged)
		{
			_fieldId = fieldId;
			_onChanged = onChanged;
		}

		public T this[int index]
		{
			get => _list[index];
			set
			{
				var oldValue = _list[index];
				_list[index] = value;
				_onChanged?.Invoke(_fieldId, new CollectionChange
				{
					Operation = CollectionOperation.Replace,
					Key = index,
					Value = value,
					OldValue = oldValue
				});
			}
		}

		public int Count => _list.Count;
		public bool IsReadOnly => false;

		public void Add(T item)
		{
			_list.Add(item);
			_onChanged?.Invoke(_fieldId, new CollectionChange
			{
				Operation = CollectionOperation.Add,
				Key = _list.Count - 1,
				Value = item
			});
		}

		public void Insert(int index, T item)
		{
			_list.Insert(index, item);
			_onChanged?.Invoke(_fieldId, new CollectionChange
			{
				Operation = CollectionOperation.Add,
				Key = index,
				Value = item
			});
		}

		public bool Remove(T item)
		{
			int index = _list.IndexOf(item);
			if (index >= 0)
			{
				_list.RemoveAt(index);
				_onChanged?.Invoke(_fieldId, new CollectionChange
				{
					Operation = CollectionOperation.Remove,
					Key = index,
					Value = item
				});
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			var item = _list[index];
			_list.RemoveAt(index);
			_onChanged?.Invoke(_fieldId, new CollectionChange
			{
				Operation = CollectionOperation.Remove,
				Key = index,
				Value = item
			});
		}

		public void Clear()
		{
			_list.Clear();
			_onChanged?.Invoke(_fieldId, new CollectionChange
			{
				Operation = CollectionOperation.Clear
			});
		}

		public bool Contains(T item) => _list.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
		public int IndexOf(T item) => _list.IndexOf(item);
		public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		// 应用变更（用于反序列化）
		public void ApplyChanges(List<CollectionChange> changes)
		{
			TypeSerializers.ApplyListChanges(_list, changes);
		}

		// 获取内部列表（用于类型匹配）
		internal List<T> GetInternalList() => _list;
	}
} 