using UnityEngine;

public static class ColorSwatchGenerator
{
    public static (Color, Color) RandomMatchingColors(float maxHueDiff = 0.3f)
    {
        Color c1 = Random.ColorHSV(0.08f, 0.55f, 0.0f, 0.2f);
        c1.a = 1.0f;

        float H, S, V;
        Color.RGBToHSV(c1, out H, out S, out V);

        Color c2 = Random.ColorHSV(Mathf.Max(H - maxHueDiff, 0), Mathf.Min(H + maxHueDiff, 1.0f), S, S);
        c2.a = 1.0f;

        return (c1, c2);
    }
}
