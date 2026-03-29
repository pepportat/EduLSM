using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Main.Helpers;

public class UIState
{
    public const int MaxInputChars = 5;
    public const int LeftPanelWidth = 400;
    
    public int ScreenMiddleX { get; set; } = 600;
    public int ScreenWidth { get; set; } = 1200;
    public int ScreenHeight { get; set; } = 800;
    public string Input { get; set; } = string.Empty;
    public int CurrentStepIndex { get; set; } = 0;
    public UITab CurrentTab { get; set; } = UITab.MemTable;
    
    public void UpdateScreenHeightAndWidth()
    {
        ScreenWidth = GetScreenWidth();
        ScreenHeight = GetScreenHeight();
        ScreenMiddleX = ScreenWidth / 2;
    }
    
    public void SwitchTab()
    {
        if (IsKeyPressed(KeyboardKey.Tab)) {
            CurrentTab = CurrentTab == UITab.MemTable ? UITab.SSTable : UITab.MemTable;
        }
    }
}