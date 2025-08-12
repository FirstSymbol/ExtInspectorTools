using System;
using System.Linq;
using UnityEngine;

namespace ExtInspectorTools
{
  [Serializable]
  public class SerializableType<T> : ISerializationCallbackReceiver where T : class
  {
    [SerializeField] private string _typeName;

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

      // Find the type by FullName among all types assignable to T
      Type = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(asm => asm.GetTypes())
        .FirstOrDefault(t => t.FullName == _typeName && typeof(T).IsAssignableFrom(t));

      if (Type == null)
        Debug.LogWarning($"Could not find type with FullName '{_typeName}' assignable to {typeof(T).Name}.");
    }
  }
}