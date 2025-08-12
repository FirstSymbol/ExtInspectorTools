#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ExtInspectorTools.Editor
{
  [CustomPropertyDrawer(typeof(SerializableType<>), true)]
  public class SerializableTypeDrawer : PropertyDrawer
  {
    private const string TypeNameField = "_typeName";

    private static readonly Dictionary<Type, List<Type>> CachedAssignableTypes = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      // Get the generic type T
      var baseType = fieldInfo.FieldType.GetGenericArguments()[0];

      // Get cached assignable types
      var availableTypes = GetAssignableTypes(baseType);

      // Generate display names, disambiguating if necessary, and add "None" at the beginning
      var displayNames = new List<string> { "None" };
      var groups = availableTypes.GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.ToList());
      foreach (var type in availableTypes)
      {
        var displayName = type.Name;
        if (groups[type.Name].Count > 1) displayName = $"{type.Name} ({type.Namespace ?? "global"})";
        displayNames.Add(displayName);
      }

      // Get current type name
      var typeNameProp = property.FindPropertyRelative(TypeNameField);
      var currentTypeName = typeNameProp.stringValue;

      // Find selected index
      var selectedIndex = 0;
      if (!string.IsNullOrEmpty(currentTypeName))
      {
        var typeIndex = availableTypes.FindIndex(t => t.FullName == currentTypeName);
        if (typeIndex >= 0) selectedIndex = typeIndex + 1; // Offset by 1 for "None"
      }

      // Draw popup with display names
      var popupRect = position;
      popupRect.height = EditorGUIUtility.singleLineHeight;
      var newIndex = EditorGUI.Popup(popupRect, label.text, selectedIndex, displayNames.ToArray());

      if (newIndex != selectedIndex)
      {
        if (newIndex == 0)
          typeNameProp.stringValue = string.Empty;
        else
          typeNameProp.stringValue = availableTypes[newIndex - 1].FullName;
        property.serializedObject.ApplyModifiedProperties();
      }

      EditorGUI.EndProperty();
    }

    private static List<Type> GetAssignableTypes(Type baseType)
    {
      if (CachedAssignableTypes.TryGetValue(baseType, out var types)) return types;

      types = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(asm => asm.GetTypes())
        .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t))
        .OrderBy(t => t.Name)
        .ThenBy(t => t.Namespace)
        .ToList();

      CachedAssignableTypes[baseType] = types;
      return types;
    }
  }
}
#endif