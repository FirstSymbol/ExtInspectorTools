using System;
using System.Linq;
using UnityEngine;

namespace ExtInspectorTools
{
  [Serializable]
  public class SerializableType<T> : ISerializationCallbackReceiver, IEquatable<SerializableType<T>> where T : class
  {
    [SerializeField] private string _typeName;

    private static readonly Type[] CachedAssignableTypes;

    static SerializableType()
    {
      // Precompute assignable types at static initialization
      CachedAssignableTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(asm => asm.GetTypes())
        .Where(t => !t.IsAbstract && !t.IsInterface && typeof(T).IsAssignableFrom(t))
        .OrderBy(t => t.Name)
        .ThenBy(t => t.Namespace)
        .ToArray();
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
      _typeName = Type?.AssemblyQualifiedName;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      if (string.IsNullOrEmpty(_typeName))
      {
        Type = null;
        return;
      }

      // Primary lookup using AssemblyQualifiedName
      Type = Type.GetType(_typeName);
      if (Type != null && typeof(T).IsAssignableFrom(Type))
      {
        return;
      }

      // Fallback if not found (e.g., namespace changed)
      string simpleName;
      if (_typeName.Contains(','))
      {
        simpleName = _typeName.Split(',')[0].Trim().Split('.').LastOrDefault() ?? string.Empty;
      }
      else
      {
        simpleName = _typeName.Split('.').LastOrDefault() ?? string.Empty;
      }

      if (string.IsNullOrEmpty(simpleName))
      {
        Type = null;
        Debug.LogWarning($"Invalid type name '{_typeName}' for {typeof(T).Name}.");
        return;
      }

      var candidates = CachedAssignableTypes.Where(t => t.Name == simpleName).ToList();

      if (candidates.Count == 1)
      {
        Type = candidates[0];
        Debug.Log($"Fallback: Resolved type '{simpleName}' from old name '{_typeName}' to {Type.AssemblyQualifiedName}.");
      }
      else
      {
        Type = null;
        string msg = candidates.Count > 1 ? "Multiple candidates found" : "Could not find type";
        Debug.LogWarning($"{msg} for '{simpleName}' from old name '{_typeName}' assignable to {typeof(T).Name}. Manual selection required.");
      }
    }

    public bool Equals(SerializableType<T> other) => other != null && Type == other.Type;

    public override bool Equals(object obj) => Equals(obj as SerializableType<T>);

    public override int GetHashCode() => HashCode.Combine(_typeName, Type);
  }
}