using TMPro;
using UnityEngine;

namespace ExtInspectorTools.Utils
{
  public static class TextUtils
  {
    public static class TextMeasurement
    {
      private static TextGenerator _cachedGenerator;

      private static TextGenerator textGenerator =>
        _cachedGenerator ??= new TextGenerator();

      public static Vector2 MeasureString(string text, Font font, int fontSize,
        FontStyle fontStyle = FontStyle.Normal,
        TextAnchor anchor = TextAnchor.UpperLeft,
        float characterSpacing = 0f)
      {
        if (string.IsNullOrEmpty(text) || font == null)
          return Vector2.zero;

        // Важно: динамически создать материал, иначе font.material будет null
        if (!font.material)
          font.RequestCharactersInTexture(text, fontSize, fontStyle);

        var settings = new TextGenerationSettings
        {
          textAnchor = anchor,
          color = Color.white,
          generationExtents = new Vector2(10000, 10000), // большой размер, чтобы не обрезалось
          pivot = Vector2.zero,
          richText = false,
          font = font,
          fontSize = fontSize,
          fontStyle = fontStyle,
          lineSpacing = 1f,
          alignByGeometry = true, // самый точный расчёт кернингa и геометрии
          resizeTextForBestFit = false,
          resizeTextMinSize = 0,
          resizeTextMaxSize = 0,
          verticalOverflow = VerticalWrapMode.Overflow,
          horizontalOverflow = HorizontalWrapMode.Overflow,
          updateBounds = true
        };

        // Поддержка character spacing
        if (!Mathf.Approximately(characterSpacing, 0f))
        {
          font.RequestCharactersInTexture(text, fontSize, fontStyle);
          settings.font?.material?.SetFloat(ShaderUtilities.ID_Padding, characterSpacing);
        }

        var width = textGenerator.GetPreferredWidth(text, settings);
        var height = textGenerator.GetPreferredHeight(text, settings);

        return new Vector2(width, height);
      }
    }
  }
}