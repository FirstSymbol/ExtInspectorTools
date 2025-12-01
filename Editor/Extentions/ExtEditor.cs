using UnityEditor;
using UnityEngine;

namespace ExtInspectorTools.Editor.Extentions
{
  public static class ExtEditor
  {
    public static bool VerticalFoldout(Rect position, bool expanded, GUIContent label, float containerHeight,int borderWidth = 1, bool active = true)
    {
      Texture2D activeFoldoutIcon = EditorStyles.foldout.normal.scaledBackgrounds[0];
      Texture2D inactiveFoldoutIcon = EditorStyles.foldout.active.scaledBackgrounds[0];

      float foldoutBtnHeight = EditorGUIUtility.singleLineHeight / 1.15f;
      
      Rect bgBorderRect = new Rect(position.x, position.y, position.width, position.height + containerHeight - borderWidth);
      Rect bgInnerRect = new Rect(bgBorderRect.x + borderWidth, bgBorderRect.y + borderWidth,
        bgBorderRect.width - borderWidth*2, bgBorderRect.height - borderWidth * 2);
      
      Rect labelBorderRect = new Rect(position.x, position.y, position.width, position.height - foldoutBtnHeight);
      Rect labelInnerRect = new Rect(labelBorderRect.x + borderWidth, labelBorderRect.y + borderWidth,
        labelBorderRect.width - borderWidth * 2, labelBorderRect.height - borderWidth * 2);
      
      Rect expandBtnBorderRect = new Rect(position.x, labelBorderRect.y + labelBorderRect.height - borderWidth + containerHeight, position.width, foldoutBtnHeight);
      Rect expandBtnInnerRect = new Rect(expandBtnBorderRect.x + borderWidth, expandBtnBorderRect.y + borderWidth,
        expandBtnBorderRect.width - borderWidth * 2, expandBtnBorderRect.height - borderWidth * 2);
      
      EditorGUI.DrawRect(bgBorderRect, ExtColors.DarkBorderColor);
      EditorGUI.DrawRect(bgInnerRect, ExtColors.BgColor0_1);
      
      EditorGUI.DrawRect(labelBorderRect, ExtColors.DarkBorderColor);
      EditorGUI.DrawRect(expandBtnBorderRect, ExtColors.DarkBorderColor);
      
      if (labelInnerRect.Contains(Event.current.mousePosition))
        EditorGUI.DrawRect(labelInnerRect, ExtColors.BgColor2);
      else
        EditorGUI.DrawRect(labelInnerRect, ExtColors.BgColor1);
      
      if (expandBtnInnerRect.Contains(Event.current.mousePosition))
        EditorGUI.DrawRect(expandBtnInnerRect, ExtColors.BgColor2);
      else
        EditorGUI.DrawRect(expandBtnInnerRect, ExtColors.BgColor1);
      
      
      EditorGUI.LabelField(labelInnerRect, label, ExtStyles.MiddleCenteredStyleLabel);
      bool labelBtn = GUI.Button(labelInnerRect, "", ExtStyles.MiddleCenteredStyleLabel);
      bool expandBtn = GUI.Button(expandBtnInnerRect, "", ExtStyles.MiddleCenteredStyleLabel);
      
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
  }
}