using Core.Common;
using Main.Helpers;
using Raylib_cs;

namespace Main.UIHandlers;

public class LsmEngine
{
    public UIState UiState { get; private set; }
    public Font Font { get; private set; }

    private readonly MemTableRenderer _memTableRenderer;
    private readonly SsTableRenderer _ssTableRenderer;

    public LsmEngine(UIState uiState, ProgramOptions programOptions)
    {
        UiState = uiState;
        var dataPath = Path.Combine(programOptions.DataPath, FileConstants.DataDirectoryName);

        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        _ssTableRenderer = new SsTableRenderer(dataPath, uiState);
        _memTableRenderer = new MemTableRenderer(dataPath, programOptions.MaxMemTableCount, uiState, _ssTableRenderer);
    }

    public void InitFont(Font font)
    {
        Font = font;
        _memTableRenderer.Font = font;
        _ssTableRenderer.Font = font;
    }

    public void DrawMemTable()
    {
        _memTableRenderer.DrawMemTable();
    }

    public void DrawSsTableScreen()
    {
        _ssTableRenderer.DrawSsTableScreen();
    }
}