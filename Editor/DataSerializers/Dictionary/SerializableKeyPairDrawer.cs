#if UNITY_EDITOR
using ExtInspectorTools.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExtInspectorTools.Editor
{
  [CustomPropertyDrawer(typeof(SerializableKeyValuePair<,>))]
  public class SerializableKeyPairDrawer : PropertyDrawer
  {
    // Константы для красоты
    
    private static readonly GUIContent emptyContent = GUIContent.none;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      // 1. Поддержка prefab override, undo, multi-edit
      EditorGUI.BeginProperty(position, label, property);
      // 2. Если поле в массиве/списке — рисуем с номером элемента
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
      float keyWidth   = totalWidth * SerializableDictionaryDrawerParams.KEY_RATIO;
      float valueWidth = totalWidth - keyWidth - SerializableDictionaryDrawerParams.SPACING;
      
      Rect keyRect   = new Rect(labelRect.x,               labelRect.y, keyWidth,   labelRect.height);
      Rect valueRect = new Rect(labelRect.x + keyWidth + SerializableDictionaryDrawerParams.SPACING, labelRect.y, valueWidth, labelRect.height);

      // 6. Рисуем поля без лейблов (чтобы не было "key" и "value")
      EditorGUI.PropertyField(keyRect,   keyProp,   emptyContent);
      EditorGUI.PropertyField(valueRect, valueProp, emptyContent);
      
      EditorGUI.EndProperty(); // BeginProperty
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      return EditorGUIUtility.singleLineHeight;
    }
  }
}
#endif
