using Main.Helpers;
using Main.UIHandlers;
using Raylib_cs;
using static Main.Helpers.ArgsHelper;
using static Raylib_cs.Raylib;

namespace Main;

class Program
{
    private static void Main(string[] args)
    {
        var programOptions = ParseArgs(args);

        if (programOptions is null)
        {
            return;
        }
        
        var uiState = new UIState();
        LsmEngine engine = new LsmEngine(uiState, programOptions);
        
        SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        InitWindow(uiState.ScreenWidth, uiState.ScreenHeight, "Edu LSM");
        engine.Font = LoadFont("./resources/Roboto-Medium.ttf");
        SetTextureFilter(engine.Font.Texture, TextureFilter.Bilinear);
        
        SetTargetFPS(60);
        
        while (!WindowShouldClose())
        {
            uiState.UpdateScreenHeightAndWidth();
            uiState.SwitchTab();
            
            BeginDrawing();
                ClearBackground(new Color(3, 7, 18, 255));

                switch (uiState.CurrentTab)
                {
                    case UITab.MemTable:
                        engine.DrawMemTable();
                        break;
                    case UITab.SSTable:
                        engine.DrawSsTableScreen();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            
            EndDrawing();
        }
        
        UnloadFont(engine.Font);
        CloseWindow();
    }
}
