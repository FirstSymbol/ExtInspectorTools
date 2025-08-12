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
        Type baseType = fieldInfo.FieldType.GetGenericArguments()[0];

        // Get cached assignable types
        List<Type> availableTypes = GetAssignableTypes(baseType);

        // Generate display names, disambiguating if necessary
        var groups = availableTypes.GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.ToList());
        List<string> displayNames = new List<string>(availableTypes.Count);
        foreach (var type in availableTypes)
        {
            string displayName = type.Name;
            if (groups[type.Name].Count > 1)
            {
                displayName = $"{type.Name} ({type.Namespace ?? "global"})";
            }
            displayNames.Add(displayName);
        }

        // Get current type name
        SerializedProperty typeNameProp = property.FindPropertyRelative(TypeNameField);
        string currentTypeName = typeNameProp.stringValue;

        // Find selected index based on FullName
        int selectedIndex = 0;
        if (!string.IsNullOrEmpty(currentTypeName))
        {
            selectedIndex = availableTypes.FindIndex(t => t.FullName == currentTypeName);
            if (selectedIndex < 0) selectedIndex = 0;
        }

        // Draw popup with display names
        Rect popupRect = position;
        popupRect.height = EditorGUIUtility.singleLineHeight;
        int newIndex = EditorGUI.Popup(popupRect, label.text, selectedIndex, displayNames.ToArray());

        if (newIndex != selectedIndex)
        {
            typeNameProp.stringValue = availableTypes[newIndex].FullName;
            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }

    private static List<Type> GetAssignableTypes(Type baseType)
    {
        if (CachedAssignableTypes.TryGetValue(baseType, out var types))
        {
            return types;
        }

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