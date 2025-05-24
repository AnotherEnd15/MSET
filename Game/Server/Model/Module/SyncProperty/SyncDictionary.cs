using System;
using System.Collections;
using System.Collections.Generic;

namespace SyncFramework
{
	// 自动追踪变更的同步字典
	public class SyncDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISyncCollection where TKey : notnull
	{
		private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
		private Action<int, CollectionChange> _onChanged;
		private int _fieldId;

		public SyncDictionary()
		{
		}

		// 实现ISyncCollection接口的SetChangeCallback方法
		public void SetChangeCallback(int fieldId, Action<int, CollectionChange> onChanged)
		{
			_fieldId = fieldId;
			_onChanged = onChanged;
		}

		public TValue this[TKey key]
		{
			get => _dict[key];
			set
			{
				bool exists = _dict.ContainsKey(key);
				var oldValue = exists ? _dict[key] : default(TValue);
				_dict[key] = value;
				_onChanged?.Invoke(_fieldId, new CollectionChange
				{
					Operation = exists ? CollectionOperation.Replace : CollectionOperation.Add,
					Key = key,
					Value = value,
					OldValue = oldValue
				});
			}
		}

		public ICollection<TKey> Keys => _dict.Keys;
		public ICollection<TValue> Values => _dict.Values;
		public int Count => _dict.Count;
		public bool IsReadOnly => false;

		public void Add(TKey key, TValue value)
		{
			_dict.Add(key, value);
			_onChanged?.Invoke(_fieldId, new CollectionChange
			{
				Operation = CollectionOperation.Add,
				Key = key,
				Value = value
			});
		}

		public bool Remove(TKey key)
		{
			if (_dict.TryGetValue(key, out var value))
			{
				_dict.Remove(key);
				_onChanged?.Invoke(_fieldId, new CollectionChange
				{
					Operation = CollectionOperation.Remove,
					Key = key,
					Value = value
				});
				return true;
			}
			return false;
		}

		public void Clear()
		{
			_dict.Clear();
			_onChanged?.Invoke(_fieldId, new CollectionChange
			{
				Operation = CollectionOperation.Clear
			});
		}

		public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
		public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);
		public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
		public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict).Contains(item);
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict).CopyTo(array, arrayIndex);
		public bool Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict).Remove(item);
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		// 应用变更（用于反序列化）
		public void ApplyChanges(List<CollectionChange> changes)
		{
			TypeSerializers.ApplyDictionaryChanges(_dict, changes);
		}

		// 获取内部字典（用于类型匹配）
		internal Dictionary<TKey, TValue> GetInternalDictionary() => _dict;
	}
} 