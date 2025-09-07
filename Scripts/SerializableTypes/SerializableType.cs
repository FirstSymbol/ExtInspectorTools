using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtInspectorTools
{
  [Serializable]
  public class SerializableType<T> : ISerializationCallbackReceiver, IEquatable<SerializableType<T>> where T : class
  {
    [SerializeField] private string _typeName;

    private static readonly Type[] CachedAssignableTypes;
    private static readonly Dictionary<string, Type> TypeNameToTypeCache = new();

    static SerializableType()
    {
      // Precompute assignable types at static initialization
      CachedAssignableTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(asm => asm.GetTypes())
        .Where(t => !t.IsAbstract && !t.IsInterface && typeof(T).IsAssignableFrom(t))
        .OrderBy(t => t.Name)
        .ThenBy(t => t.Namespace)
        .ToArray();

      // Populate type name cache
      foreach (var type in CachedAssignableTypes)
      {
        if (!string.IsNullOrEmpty(type.FullName))
          TypeNameToTypeCache[type.FullName] = type;
      }
    }

    public SerializableType()
    {
    }

    public SerializableType(Type initialType)
    {
      if (initialType != null && !typeof(T).IsAssignableFrom(initialType))
        throw new ArgumentException($"Type {initialType} must be assignable to {typeof(T)}.");
      Type = initialType;
    }

    public Type Type { get; private set; }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
      _typeName = Type?.FullName;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      if (string.IsNullOrEmpty(_typeName))
      {
        Type = null;
        return;
      }

      // Use cached dictionary for faster lookup
      if (TypeNameToTypeCache.TryGetValue(_typeName, out var type))
      {
        Type = type;
      }
      else
      {
        Type = null;
        Debug.LogWarning($"Could not find type with FullName '{_typeName}' assignable to {typeof(T).Name}.");
      }
    }

    public bool Equals(SerializableType<T> other) => other != null && Type == other.Type;

    public override bool Equals(object obj) => Equals(obj as SerializableType<T>);

    public override int GetHashCode() => HashCode.Combine(_typeName, Type);
  }
}