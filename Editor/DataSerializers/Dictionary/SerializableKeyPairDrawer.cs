#if UNITY_EDITOR
using ExtInspectorTools.Data;
using UnityEditor;
using UnityEngine;

namespace ExtInspectorTools.Editor
{
  [CustomPropertyDrawer(typeof(SerializableKeyValuePair<,>))]
  public class SerializableKeyPairDrawer : PropertyDrawer
  {
    internal float KeyRatio;
    internal float Spacing;
    
    private static readonly GUIContent emptyContent = GUIContent.none;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      KeyRatio = SerializableDictionaryDrawerParams.KEY_RATIO;
      Spacing = SerializableDictionaryDrawerParams.SPACING;
      
      EditorGUI.BeginProperty(position, label, property);
      label = EditorGUI.BeginProperty(position, label, property);
      Rect labelRect = EditorGUI.PrefixLabel(position, label);
      
      var enabledProp = property.FindPropertyRelative("isCorrect");
      bool isCorrect = enabledProp.boolValue;
      
      Color oldBg = GUI.backgroundColor;
      Color oldColor = GUI.color;
      
      if (!isCorrect)
      {
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f, 1f);
        GUI.color = Color.white;
      }
      else
      {
        GUI.backgroundColor = oldBg;
        GUI.color = oldColor;;
      }
      
      // 3. Отключаем лишние лейблы внутри полей
      EditorGUIUtility.labelWidth = 0f;

      // 4. Находим свойства
      SerializedProperty keyProp   = property.FindPropertyRelative("key");
      SerializedProperty valueProp = property.FindPropertyRelative("value");

      if (keyProp == null || valueProp == null)
      {
        EditorGUI.LabelField(position, "Error: fields not found");
        EditorGUI.EndProperty();
        return;
      }

      // 5. Делим пространство
      float totalWidth = labelRect.width;
      float keyWidth   = totalWidth * KeyRatio;
      float valueWidth = totalWidth - keyWidth - Spacing;
      
      Rect keyRect   = new Rect(labelRect.x,               labelRect.y, keyWidth,   labelRect.height);
      Rect valueRect = new Rect(labelRect.x + keyWidth + Spacing, labelRect.y, valueWidth, labelRect.height);

      // 6. Рисуем поля без лейблов (чтобы не было "key" и "value")
      EditorGUI.PropertyField(keyRect,   keyProp, emptyContent, true);
      EditorGUI.PropertyField(valueRect, valueProp, emptyContent,true);
      
      EditorGUI.EndProperty(); // BeginProperty
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      SerializedProperty keyProp   = property.FindPropertyRelative("key");
      SerializedProperty valueProp = property.FindPropertyRelative("value");
      float height = EditorGUIUtility.singleLineHeight;
      if (keyProp == null || valueProp == null)
      {
        Debug.Log("Error: fields not found");
        return height;
      }
      float keyH = EditorGUI.GetPropertyHeight(keyProp, true);
      float valueH = EditorGUI.GetPropertyHeight(valueProp, true);
      
      return keyH > valueH ? keyH : valueH;;
    }
  }
}
#endif
