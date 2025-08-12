using System;
using System.Linq;
using UnityEngine;

namespace ExtInspectorTools
{
  [Serializable]
  public class SerializableType<T> : ISerializationCallbackReceiver where T : class
  {
    [SerializeField] private string _typeName;
    private Type _type;

    public Type Type => _type;

    public SerializableType() { }

    public SerializableType(Type initialType)
    {
      if (initialType != null && !typeof(T).IsAssignableFrom(initialType))
      {
        throw new ArgumentException($"Type {initialType} must be assignable to {typeof(T)}.");
      }
      _type = initialType;
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
      _typeName = _type?.FullName;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      if (string.IsNullOrEmpty(_typeName))
      {
        _type = null;
        return;
      }

      // Find the type by FullName among all types assignable to T
      _type = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(asm => asm.GetTypes())
        .FirstOrDefault(t => t.FullName == _typeName && typeof(T).IsAssignableFrom(t));

      if (_type == null)
      {
        Debug.LogWarning($"Could not find type with FullName '{_typeName}' assignable to {typeof(T).Name}.");
      }
    }
  }
}
