using System;
using UnityEngine;
using ExtInspectorTools;

namespace ExtInspectorTools
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
  public class RequireNotNullAttribute : PropertyAttribute { }
}

#if UNITY_EDITOR

#endif