using System;
using System.Collections;
using System.Collections.Generic;
using ExtInspectorTools.Data;
using UnityEngine;

namespace ExtInspectorTools
{
  [Serializable]
  public class SerializableDictionary<TKey,TValue> : IDictionary<TKey,TValue>, ISerializationCallbackReceiver
  {
    [SerializeField] List<SerializableKeyValuePair<TKey,TValue>> pairs = new();
    [NonSerialized] Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
    [NonSerialized] private HashSet<int> duplicateIndices = new();
    
    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
      dictionary.Clear();
      duplicateIndices.Clear();

      for (int i = 0; i < pairs.Count; i++)
      {
        if (!dictionary.ContainsKey(pairs[i].key))
        {
          SetCorrectInPairs(i, true);
          
          dictionary.Add(pairs[i].key, pairs[i].value);
        }
        else
        {
          SetCorrectInPairs(i, false);

          duplicateIndices.Add(i);
        }
      }
    }

    private void SetCorrectInPairs(int i, bool value)
    {
      var t = pairs[i];
      t.SetCorrect(value);
      pairs[i] = t;
    }

    #region IDictionary Interface

    public ICollection<TKey> Keys
    {
      get
      {
        ValidateDictionary();
        return dictionary.Keys;
      }
    }

    public ICollection<TValue> Values
    {
      get
      {
        ValidateDictionary();
        return dictionary.Values;
      }
    }

    public int Count
    {
      get
      {
        ValidateDictionary();
        return dictionary.Count;
      }
    }
    
    public TValue this[TKey key]
    {
      get
      {
        if (dictionary == null) throw new KeyNotFoundException();
        return dictionary[key];
      }
      set
      {
        ValidateDictionary();
        dictionary[key] = value;
      }
    }
    
    public bool IsReadOnly => false;
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      ValidateDictionary();
      return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      ValidateDictionary();
      return dictionary.GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      ValidateDictionary();
      dictionary.Add(item.Key, item.Value);
    }

    private void ValidateDictionary()
    {
      dictionary ??= new Dictionary<TKey, TValue>();
    }

    public void Clear()
    {
      ValidateDictionary();
      dictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      ValidateDictionary();
      return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      ValidateDictionary();
      (dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      ValidateDictionary();
      return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
    }

    
    public void Add(TKey key, TValue value)
    {
      ValidateDictionary();
      dictionary.Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
      ValidateDictionary();
      return dictionary.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
      ValidateDictionary();
      return dictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      ValidateDictionary();
      return dictionary.TryGetValue(key, out value);
    }

    
    #endregion
  }
}