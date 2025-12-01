using UnityEditor;
using UnityEngine;

namespace ExtInspectorTools.Editor.Extentions
{
  public static class ExtColors
  {
    public static Color BgColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.76f, 0.76f, 0.76f, 1f);
    public static Color DarkBorderColor = new Color(0.141f, 0.141f, 0.141f);
    public static Color FieldBackgroundColor = EditorGUIUtility.isProSkin ? new Color(0.165f, 0.165f, 0.165f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
    public static Color BgColor0 = new Color(0.254f, 0.254f, 0.254f, 1f);
    public static Color BgColor0_1 = new Color(0.243f, 0.243f, 0.243f, 1f);
    public static Color BgColor1 = new Color(0.317f, 0.317f, 0.317f, 1f);
    public static Color BgColor2 = new Color(0.403f, 0.403f, 0.403f, 1f);
    public static Color BgColor3 = new Color(0.6f, 0.6f, 0.6f, 1f);
  }
}