namespace Main.Components.Helpers;

public static class RoundnessHelper
{
    public static float Roundness(float width, float height, float targetRadiusPx)
    {
        return targetRadiusPx * 2f / Math.Min(width, height);
    }
}