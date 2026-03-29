using Raylib_cs;

namespace Main.Helpers;

public static class CustomGridHelper
{
    public static void DrawGridCustom(int slices, float spacing, Color color)
    {
        int halfSlices = slices/2;

        Rlgl.Begin(DrawMode.Lines);
        for (var i = -halfSlices; i <= halfSlices; i++)
        {
            Rlgl.Color3f(color.R / 255f, color.G / 255f, color.B / 255f);
                

            Rlgl.Vertex3f(i*spacing, 0.0f, -halfSlices*spacing);
            Rlgl.Vertex3f(i*spacing, 0.0f, halfSlices*spacing);

            Rlgl.Vertex3f(-halfSlices*spacing, 0.0f, i*spacing);
            Rlgl.Vertex3f(halfSlices*spacing, 0.0f, i*spacing);
        }
        Rlgl.End();
    }
}