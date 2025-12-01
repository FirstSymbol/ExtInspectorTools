using UnityEditor;

namespace ExtInspectorTools.Editor
{
  internal static class SerializableDictionaryDrawerParams
  {
    internal static float KEY_RATIO   = 0.3f;
    internal static float SPACING     = 4f;
    internal static float VALUE_RATIO => 1 - KEY_RATIO;
  }
}