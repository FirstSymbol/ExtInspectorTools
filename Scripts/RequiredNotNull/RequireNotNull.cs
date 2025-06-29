using System;
using UnityEngine;

namespace ExtInspectorTools
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
  public class RequireNotNullAttribute : PropertyAttribute { }
}

#if UNITY_EDITOR

#endif