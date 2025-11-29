#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExtInspectorTools.Editor
{
  [InitializeOnLoad]
  public static class RequireNotNullValidator
  {
    [Obsolete("Obsolete")]
    static RequireNotNullValidator()
    {
      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [Obsolete("Obsolete")]
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
      if (state == PlayModeStateChange.ExitingEditMode)
      {
        // Проверяем все объекты на сцене перед запуском
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
        {
          foreach (MonoBehaviour mono in go.GetComponents<MonoBehaviour>())
          {
            if (mono == null) continue;

            var fields = mono.GetType().GetFields(System.Reflection.BindingFlags.Instance | 
                                                  System.Reflection.BindingFlags.Public | 
                                                  System.Reflection.BindingFlags.NonPublic);

            foreach (var field in fields)
            {
              if (field.IsDefined(typeof(RequireNotNullAttribute), true) && 
                  field.GetValue(mono) == null)
              {
                Debug.LogError($"The {field.Name} in the {mono.GetType().Name} on the object {go.name} can't be null!", mono);
                EditorApplication.isPlaying = false;
                return;
              }
            }
          }
        }
      }
    }
  }
}
#endif