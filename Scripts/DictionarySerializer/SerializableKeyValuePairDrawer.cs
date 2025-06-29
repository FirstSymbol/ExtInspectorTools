#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectContent.Code.ToolsAndExtentionsScripts.DictionarySerializer
{
  [CustomPropertyDrawer(typeof(SerializableKeyValuePair<,>))]
  public class SerializableKeyValuePairDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var keyProperty = property.FindPropertyRelative("Key");
      var valueProperty = property.FindPropertyRelative("Value");

      var keyRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
      var valueRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height);

      EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
      EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

      EditorGUI.EndProperty();
    }
  }
}
#endif