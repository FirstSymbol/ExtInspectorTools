#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ExtInspectorTools.Editor
{
  [CustomPropertyDrawer(typeof(ExtInspectorTools.TypeReference<>))]
  public class TypeReferenceDrawer : PropertyDrawer
  {
    private static readonly Dictionary<Type, List<Type>> CachedDerivedTypes = new();
    private static readonly GUIContent NoneOption = new("None");

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);
      var typeNameProperty = property.FindPropertyRelative("typeName");
      string currentTypeName = typeNameProperty.stringValue;

      Type baseType = fieldInfo.FieldType.GenericTypeArguments[0];
      var derivedTypes = GetDerivedTypes(baseType);

      // Формируем список отображаемых имён (только имя типа)
      var displayNames = derivedTypes.Select(t => new GUIContent(t.Name)).ToList();
      displayNames.Insert(0, NoneOption);

      int selectedIndex = string.IsNullOrEmpty(currentTypeName) ? 0 : 
        derivedTypes.FindIndex(t => t.AssemblyQualifiedName == currentTypeName) + 1;

      EditorGUI.BeginChangeCheck();
      selectedIndex = EditorGUI.Popup(position, label, selectedIndex, displayNames.ToArray());
      if (EditorGUI.EndChangeCheck())
      {
        typeNameProperty.stringValue = selectedIndex == 0 ? null : derivedTypes[selectedIndex - 1].AssemblyQualifiedName;
        property.serializedObject.ApplyModifiedProperties();
      }

      EditorGUI.EndProperty();
    }

    private static List<Type> GetDerivedTypes(Type baseType)
    {
      if (CachedDerivedTypes.TryGetValue(baseType, out var types))
      {
        return types;
      }

      types = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
        .OrderBy(t => t.Name) // Сортировка только по имени типа
        .ToList();

      CachedDerivedTypes[baseType] = types;
      return types;
    }

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
      CachedDerivedTypes.Clear();
      EditorApplication.delayCall += () => CachedDerivedTypes.Clear();
    }
  }
}
#endif