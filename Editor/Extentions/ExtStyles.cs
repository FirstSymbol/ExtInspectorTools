using UnityEditor;
using UnityEngine;

namespace ExtInspectorTools.Editor.Extentions
{
  public static class ExtStyles
  {
    public static GUIStyle MiddleCenteredStyleLabel = new(EditorStyles.label) {
      alignment = TextAnchor.MiddleCenter
    };
  }
}