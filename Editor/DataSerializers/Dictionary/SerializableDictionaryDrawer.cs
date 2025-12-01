using ExtInspectorTools.Editor.Extentions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ExtInspectorTools.Editor
{
  [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
  public class SerializableDictionaryDrawer : PropertyDrawer
  {
    private class Rects
    {
      public Rect foldoutRect = default;
      public Rect foldoutContainerRect = default;
      
      public static void SetHeightRect(ref Rect rect, float value) => 
        rect.height = value;
    }

    #region Fields

    private ReorderableList list;
    private float slider_value = 0.5f;
    private float key_ratio => slider_value;
    private float value_ratio => 1 - slider_value;
    private SerializedProperty _keyRatio;
    private SerializedProperty pairs;
    
    private static float foldoutBtn = EditorGUIUtility.singleLineHeight / 1.15f;
    private static float sliderHeight = EditorGUIUtility.singleLineHeight;
    private static Rects rects = new Rects();
    
    private readonly Vector2 offset = new Vector2(0, 2);
    private readonly float sliderPaddingTop = 2;

    #endregion

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);
      
      _keyRatio ??= property.FindPropertyRelative("_editorKeyRatio");
      slider_value = _keyRatio.floatValue;
      
      Rect subRect = new Rect(position.x + offset.x, position.y + offset.y, position.width - offset.x, position.height - offset.y);
      
      property.serializedObject.Update();
      pairs ??= property.FindPropertyRelative("pairs");
      ValidateList(pairs);
      
      rects.foldoutRect = new Rect(subRect.x, subRect.y, subRect.width, EditorGUIUtility.singleLineHeight + foldoutBtn);
      property.isExpanded = ExtEditor.VerticalFoldout(rects.foldoutRect, property.isExpanded, label, rects.foldoutContainerRect.height);
      
      rects.foldoutContainerRect = new Rect(rects.foldoutRect.x+6, position.y + rects.foldoutRect.height - foldoutBtn,
        rects.foldoutRect.width-12, 0);
      
      if (property.isExpanded)
      {
        Rects.SetHeightRect(ref rects.foldoutContainerRect, list.GetHeight() + sliderHeight + sliderPaddingTop);
        DrawFoldoutRect(rects.foldoutContainerRect, property, label);
      }
      else
        Rects.SetHeightRect(ref rects.foldoutContainerRect, 0);

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      float sum = 0f;
      sum += rects.foldoutRect.height;
      sum += rects.foldoutContainerRect.height;
      sum += offset.y;
      return sum;
    }
    private void DrawFoldoutRect(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginChangeCheck();
      
      Rect sliderRect = new Rect(position.x + 15, position.y + sliderPaddingTop, position.width - 15, EditorGUIUtility.singleLineHeight);
      Rect listRect = new Rect(position.x, sliderRect.y + sliderHeight, position.width, list.GetHeight());
      
      EditorGUI.BeginChangeCheck();
      var t = GUI.HorizontalSlider(sliderRect, slider_value, 0f, 1f);
      if (EditorGUI.EndChangeCheck())
      {
        slider_value = t;
        _keyRatio.floatValue = t;
        property.serializedObject.ApplyModifiedProperties();
      }
      
      pairs ??= property.FindPropertyRelative("pairs");
      ValidateList(pairs);
      list.DoList(listRect);
      pairs.serializedObject.ApplyModifiedProperties();
    }
    
    private void ValidateList(SerializedProperty property)
    {
      if (list == null)
      {
        list = new ReorderableList(property.serializedObject, property, true, true, true, true);
        
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
          float oldValue = SerializableDictionaryDrawerParams.KEY_RATIO;
          SerializableDictionaryDrawerParams.KEY_RATIO = slider_value;
          
          SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
          EditorGUI.PropertyField(rect, element, GUIContent.none, true); // includeChildren true - включает под-свойства и drawer'ы
          
          SerializableDictionaryDrawerParams.KEY_RATIO =  oldValue;
        };
        
        list.drawHeaderCallback = (Rect rect) =>
        {
          Rect cRect = new Rect(rect.x + 15, rect.y, rect.width - 15, rect.height);
          
          float keyWidth = cRect.width * key_ratio;
          float valueWidth = cRect.width * value_ratio;
          
          Rect keyRect = new Rect(cRect.x, cRect.y, keyWidth, cRect.height);
          Rect valueRect = new Rect(keyRect.x + keyRect.width + SerializableDictionaryDrawerParams.SPACING+2, cRect.y, valueWidth, cRect.height);
          Rect separatorRect = new Rect(keyRect.x + keyWidth + (SerializableDictionaryDrawerParams.SPACING / 2)-1, cRect.y, 1, cRect.height);
          
          EditorGUI.LabelField(keyRect, "Key", ExtStyles.MiddleCenteredStyleLabel);
          EditorGUI.LabelField(valueRect, "Value", ExtStyles.MiddleCenteredStyleLabel);
          EditorGUI.DrawRect(separatorRect, ExtColors.DarkBorderColor);
        };
      }
    }
    
  }

  
}