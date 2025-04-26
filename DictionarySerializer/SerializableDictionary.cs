using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectContent.Code.ToolsAndExtentionsScripts.DictionarySerializer
{
  [Serializable]
  public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable
  {
    [SerializeField] private List<SerializableKeyValuePair<TKey, TValue>> list = new();

    private Dictionary<TKey, TValue> dictionary = new();

    public int Count => dictionary.Count;

    public TValue this[TKey key]
    {
      get => dictionary[key];
      set
      {
        dictionary[key] = value;
        var existing = list.Find(kvp => EqualityComparer<TKey>.Default.Equals(kvp.Key, key));
        if (existing != null)
          existing.Value = value;
        else
          list.Add(new SerializableKeyValuePair<TKey, TValue> { Key = key, Value = value });
      }
    }

    public IEnumerator GetEnumerator()
    {
      throw new NotImplementedException();
    }

    public void OnBeforeSerialize()
    {
      // Список уже обновляется при изменении словаря, поэтому здесь ничего не делаем
    }

    public void OnAfterDeserialize()
    {
      dictionary.Clear();
      foreach (var kvp in list)
        if (kvp.Key != null && !dictionary.ContainsKey(kvp.Key))
          dictionary[kvp.Key] = kvp.Value;
    }

    public void Add(TKey key, TValue value)
    {
      dictionary.Add(key, value);
      list.Add(new SerializableKeyValuePair<TKey, TValue> { Key = key, Value = value });
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      return dictionary.TryGetValue(key, out value);
    }

    public void Clear()
    {
      dictionary.Clear();
      list.Clear();
    }

    public bool ContainsKey(TKey key)
    {
      return dictionary.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
      if (dictionary.Remove(key))
      {
        list.RemoveAll(kvp => EqualityComparer<TKey>.Default.Equals(kvp.Key, key));
        return true;
      }

      return false;
    }
  }
}