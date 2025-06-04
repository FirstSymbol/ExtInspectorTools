#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectContent.Code.ToolsAndExtentionsScripts.RequiredNotNull
{
  [InitializeOnLoad]
  public static class RequireNotNullValidator
  {
    static RequireNotNullValidator()
    {
      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
      if (state == PlayModeStateChange.ExitingEditMode)
      {
        // Проверяем все объекты на сцене перед запуском
        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
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