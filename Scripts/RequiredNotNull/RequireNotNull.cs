using System;
using UnityEngine;

namespace ProjectContent.Code.ToolsAndExtentionsScripts.RequiredNotNull
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
  public class RequireNotNullAttribute : PropertyAttribute { }
}

#if UNITY_EDITOR

#endif