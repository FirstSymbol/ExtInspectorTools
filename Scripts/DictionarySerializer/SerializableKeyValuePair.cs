using System;

namespace ProjectContent.Code.ToolsAndExtentionsScripts.DictionarySerializer
{
  [Serializable]
  public class SerializableKeyValuePair<TKey, TValue>
  {
    public TKey Key;
    public TValue Value;
  }
}