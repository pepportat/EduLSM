using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Main.Components;

public static class Button
{
    public static void DrawActionButton(int x, string label, int slot, bool enabled, Font font, Action onClick)
    {
        const int w = 100, h = 40;
        int y = 10 + slot * (h + 10);
        var rect = new Rectangle(x, y, w, h);
        
        DrawRectangleRounded(rect, 0.4f, 10, enabled ? Color.Red : Color.Gray);
        var tw = MeasureTextEx(font, label, 20, 2).X;
        
        DrawTextEx(font, label, new Vector2(x + (w - tw) / 2f, y + 10), 20, 2,
            enabled ? Color.White : Color.LightGray);

        if (enabled && CheckCollisionPointRec(GetMousePosition(), rect))
        {
            DrawRectangleRoundedLines(rect, 0.4f, 10, Color.White);
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                onClick();
            }
        }
    }
}