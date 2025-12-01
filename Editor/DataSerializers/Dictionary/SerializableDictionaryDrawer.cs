using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ExtInspectorTools.Editor
{
  [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
  public class SerializableDictionaryDrawer : PropertyDrawer
  {
    private class Styles
    {
      public GUIStyle MiddleCenteredStyleLabel = new(EditorStyles.label) {
        alignment = TextAnchor.MiddleCenter
      };
    }

    private class Colors
    {
      public Color BackgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.76f, 0.76f, 0.76f, 1f);
      public Color FieldBackgroundColor = EditorGUIUtility.isProSkin ? new Color(0.165f, 0.165f, 0.165f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
    }

    private class Contents
    {
      
    }

    private class Rects
    {
      public Rect foldoutRect = default;
      public Rect foldoutContainerRect = default;
    }
    private ReorderableList list;
    
    private static Texture activeFoldoutIcon = null;
    private static Texture inactiveFoldoutIcon = null;
    private static Texture foldoutIconExpanded = null;
    
    private static float foldoutBtn = EditorGUIUtility.singleLineHeight / 1.15f;
    private static Styles styles = new Styles();
    private static Colors colors = new Colors();
    private static Rects rects = new Rects();
    
    private readonly Dictionary<string, bool> foldouts = new();
    private readonly Dictionary<string, string> searchFilters = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);
      property.serializedObject.Update();
      SerializedProperty pairs = property.FindPropertyRelative("pairs");
      ValidateList(pairs);
      
      rects.foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + foldoutBtn);
      
      property.isExpanded = DrawExtFoldout(rects.foldoutRect, property.isExpanded, label, rects.foldoutContainerRect.height);
      
      if (property.isExpanded)
      {
        rects.foldoutContainerRect = new Rect(rects.foldoutRect.x, position.y + rects.foldoutRect.height - foldoutBtn,
          rects.foldoutRect.width, list.GetHeight() + 8);
        DrawFoldoutRect(rects.foldoutContainerRect, property, label);
      }
      else
      {
        rects.foldoutContainerRect = new Rect(rects.foldoutRect.x, position.y + rects.foldoutRect.height - foldoutBtn,
          rects.foldoutRect.width, 0);
      }
      
      EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      float sum = 0f;
      sum += rects.foldoutRect.height;
      sum += rects.foldoutContainerRect.height;
      return sum;
    }
    
    public static bool DrawExtFoldout(Rect position, bool expanded, GUIContent label, float containerHeight,int borderWidth = 1, bool active = true)
    {
      activeFoldoutIcon ??= EditorStyles.foldout.normal.scaledBackgrounds[0];
      inactiveFoldoutIcon ??= EditorStyles.foldout.active.scaledBackgrounds[0];

      float foldoutBtnHeight = EditorGUIUtility.singleLineHeight / 1.15f;
      
      GUIStyle titleStyle = new GUIStyle(EditorStyles.label)
      {
        alignment = TextAnchor.MiddleCenter,
      };

      Rect labelBorderRect = new Rect(position.x, position.y, position.width, position.height - foldoutBtnHeight);
      Rect labelInnerRect = new Rect(labelBorderRect.x + borderWidth, labelBorderRect.y + borderWidth,
        labelBorderRect.width - borderWidth * 2, labelBorderRect.height - borderWidth * 2);
      
      Rect expandBtnBorderRect = new Rect(position.x, labelBorderRect.y + labelBorderRect.height - borderWidth + containerHeight, position.width, foldoutBtnHeight);
      Rect expandBtnInnerRect = new Rect(expandBtnBorderRect.x + borderWidth, expandBtnBorderRect.y + borderWidth,
        expandBtnBorderRect.width - borderWidth * 2, expandBtnBorderRect.height - borderWidth * 2);
      
      EditorGUI.DrawRect(labelBorderRect, Color.black);
      EditorGUI.DrawRect(labelInnerRect, colors.FieldBackgroundColor);
      
      EditorGUI.LabelField(labelInnerRect, label, titleStyle);
      
      EditorGUI.DrawRect(expandBtnBorderRect, Color.black);
      EditorGUI.DrawRect(expandBtnInnerRect, colors.FieldBackgroundColor);
      
      bool labelBtn = GUI.Button(labelInnerRect, "", styles.MiddleCenteredStyleLabel);
      bool expandBtn = GUI.Button(expandBtnInnerRect, "", styles.MiddleCenteredStyleLabel);
      
      if (expanded)
      {
        GUIUtility.RotateAroundPivot(-90, expandBtnInnerRect.center);
        if (active)
          GUI.DrawTexture(expandBtnInnerRect, activeFoldoutIcon, ScaleMode.ScaleToFit);
        else
          GUI.DrawTexture(expandBtnInnerRect, inactiveFoldoutIcon, ScaleMode.ScaleToFit);
        GUIUtility.RotateAroundPivot(90, expandBtnInnerRect.center);
      }
      else
      {
        GUIUtility.RotateAroundPivot(90, expandBtnInnerRect.center);
        if (active)
          GUI.DrawTexture(expandBtnInnerRect, activeFoldoutIcon, ScaleMode.ScaleToFit);
        else
          GUI.DrawTexture(expandBtnInnerRect, inactiveFoldoutIcon, ScaleMode.ScaleToFit);
        GUIUtility.RotateAroundPivot(-90, expandBtnInnerRect.center);
      }
      
      if (labelBtn || expandBtn) 
        expanded = !expanded;
        
      return expanded;
    }
    
    private void DrawFoldoutRect(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginChangeCheck();
      
      Rect listRect = new Rect(position.x, position.y + 4, position.width, EditorGUIUtility.singleLineHeight);
      
      SerializedProperty pairs = property.FindPropertyRelative("pairs");
      ValidateList(pairs);
      list.DoList(listRect);
        pairs.serializedObject.ApplyModifiedProperties();

      EditorGUI.EndChangeCheck();
    }

    private void ValidateList(SerializedProperty property)
    {
      if (list == null)
      {
        list = new ReorderableList(property.serializedObject, property, true, true, true, true);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
          SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
          EditorGUI.PropertyField(rect, element, GUIContent.none, true); // includeChildren true - включает под-свойства и drawer'ы
        };
        list.drawHeaderCallback = (Rect rect) =>
        {
          Color color = new Color(0.141f, 0.141f, 0.141f);
          
          Rect cRect = new Rect(rect.x + 15, rect.y, rect.width - 15, rect.height);
          
          float keyWidth = cRect.width * SerializableDictionaryDrawerParams.KEY_RATIO;
          float valueWidth = cRect.width * SerializableDictionaryDrawerParams.VALUE_RATIO;
          
          Rect keyRect = new Rect(cRect.x, cRect.y, keyWidth, cRect.height);
          Rect valueRect = new Rect(keyRect.x + keyRect.width + SerializableDictionaryDrawerParams.SPACING+2, cRect.y, valueWidth, cRect.height);
          Rect separatorRect = new Rect(keyRect.x + keyWidth + (SerializableDictionaryDrawerParams.SPACING / 2)-1, cRect.y, 1, cRect.height);
          
          EditorGUI.LabelField(keyRect, "Key", styles.MiddleCenteredStyleLabel);
          EditorGUI.LabelField(valueRect, "Value", styles.MiddleCenteredStyleLabel);
          EditorGUI.DrawRect(separatorRect, color);
        };
      }
    }
    
  }

  
}