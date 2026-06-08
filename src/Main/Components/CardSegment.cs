using Raylib_cs;
using static Raylib_cs.Raylib;
using static Main.Components.Helpers.RoundnessHelper;

namespace Main.Components;

public static class CardSegment
{
    public static void DrawCardSegment(int xc, int yc, int width, int height, Color bgColor, Color borderColor)
    {
        DrawRectangleRounded(new Rectangle(xc, yc, width, height), Roundness(width, height, 8f), 8, bgColor);
        DrawRectangleRoundedLinesEx(new Rectangle(xc, yc, width, height), Roundness(width, height, 8f), 8, 2, borderColor);
    }
}