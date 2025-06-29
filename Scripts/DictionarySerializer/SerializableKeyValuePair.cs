using System;

namespace ExtInspectorTools
{
  [Serializable]
  public class SerializableKeyValuePair<TKey, TValue>
  {
    public TKey Key;
    public TValue Value;
  }
}