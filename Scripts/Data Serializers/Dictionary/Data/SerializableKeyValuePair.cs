using System;
using UnityEngine;

namespace ExtInspectorTools.Data
{
  [Serializable]
  public struct SerializableKeyValuePair<TKey, TValue>
  {
    public TKey key;
    public TValue value;
    [HideInInspector] public bool isCorrect;

    public SerializableKeyValuePair(TKey key, TValue value)
    {
      this.key = key;
      this.value = value;
      isCorrect = true;
    }

    public void SetCorrect(bool value)
    {
      isCorrect = value;
    }
  }
}