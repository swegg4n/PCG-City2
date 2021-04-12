using UnityEngine;

public static class ColorSwatchGenerator
{
    public static (Color, Color) RandomMatchingColors(float hueDiff = 0.3f)
    {
        Color c1 = Random.ColorHSV(0.08f, 0.55f, 0.0f, 0.2f);
        c1.a = 1.0f;

        float H, S, V;
        Color.RGBToHSV(c1, out H, out S, out V);
        float H2 = (Random.Range(0.0f, 1.0f) < 0.5) ? Mathf.Max(H - hueDiff, 0.0f) : Mathf.Min(H + hueDiff, 1.0f);

        Color c2 = Color.HSVToRGB(H2, S, V);

        return (c1, c2);
    }
}
