using System;
using UnityEngine;
using ExtInspectorTools;

namespace ExtInspectorTools
{
  [Serializable]
  public class TypeReference<T> : ISerializationCallbackReceiver
  {
    [SerializeField]
    private string typeName;

    private Type _type;
    public Type Type
    {
      get
      {
        if (_type == null && !string.IsNullOrEmpty(typeName))
        {
          _type = Type.GetType(typeName);
        }
        return _type;
      }
      set
      {
        if (value != null && typeof(T).IsAssignableFrom(value) && !value.IsAbstract && !value.IsInterface)
        {
          _type = value;
          typeName = value.AssemblyQualifiedName;
        }
        else
        {
          _type = null;
          typeName = null;
        }
      }
    }

    public TypeReference() { }
    public TypeReference(Type type) => Type = type;

    // Реализация ISerializationCallbackReceiver
    public void OnBeforeSerialize()
    {
      // Ничего не делаем перед сериализацией
    }

    public void OnAfterDeserialize()
    {
      // Сбрасываем _type после десериализации, чтобы оно пересчиталось при следующем обращении
      _type = null;
    }
  }
}