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

    private static readonly Dictionary<Type, (List<Type> Types, List<string> DisplayNames)> CachedTypeData = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      // Get the generic type T
      var baseType = fieldInfo.FieldType.IsArray
        ? fieldInfo.FieldType.GetElementType().GetGenericArguments()[0]
        : fieldInfo.FieldType.GetGenericArguments()[0];

      // Get cached types and display names
      var (availableTypes, displayNames) = GetTypeData(baseType);

      // Get current type name (now AssemblyQualifiedName)
      var typeNameProp = property.FindPropertyRelative(TypeNameField);
      var currentTypeName = typeNameProp.stringValue;

      // Find selected index
      var selectedIndex = 0;
      if (!string.IsNullOrEmpty(currentTypeName))
      {
        var typeIndex = availableTypes.FindIndex(t => t.AssemblyQualifiedName == currentTypeName);
        if (typeIndex >= 0) selectedIndex = typeIndex + 1; // Offset by 1 for "None"
      }

      // Draw popup with display names
      var popupRect = position;
      popupRect.height = EditorGUIUtility.singleLineHeight;
      var newIndex = EditorGUI.Popup(popupRect, label.text, selectedIndex, displayNames.ToArray());

      if (newIndex != selectedIndex)
      {
        typeNameProp.stringValue = newIndex == 0 ? string.Empty : availableTypes[newIndex - 1].AssemblyQualifiedName;
        property.serializedObject.ApplyModifiedProperties();
      }

      EditorGUI.EndProperty();
    }

    private static (List<Type> Types, List<string> DisplayNames) GetTypeData(Type baseType)
    {
      if (CachedTypeData.TryGetValue(baseType, out var data))
        return data;

      // Compute assignable types
      var types = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(asm => asm.GetTypes())
        .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t))
        .OrderBy(t => t.Name)
        .ThenBy(t => t.Namespace)
        .ToList();

      // Generate display names
      var displayNames = new List<string> { "None" };
      var groups = types.GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.ToList());
      foreach (var type in types)
      {
        var displayName = type.Name;
        if (groups[type.Name].Count > 1)
          displayName = $"{type.Name} ({type.Namespace ?? "global"})";
        displayNames.Add(displayName);
      }

      // Cache the results
      data = (types, displayNames);
      CachedTypeData[baseType] = data;
      return data;
    }
  }
}
#endif