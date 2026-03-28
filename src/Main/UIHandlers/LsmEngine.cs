using Core.MemTables;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Main.Helpers;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Main.UIHandlers;

public partial class LsmEngine
{
    private UIState UiState { get; set; }
    private IMemTable Tree { get; set;}
    private Dictionary<int, NodeSnapshot> Layout { get; set; }
    private List<MemTableStep> Steps { get; set; }
    public Font Font { get; set; }
    
    public LsmEngine(UIState uiState)
    {
        UiState = uiState;
        Tree = new RedBlackTree();
        Layout = Tree.GetLayout();
        Steps = [];
    }

    public void Insert(int key, string value)
    {
        var (_, list) = Tree.Add(key, $"Data for key {value}");
        Steps = list;
        UpdateLayout(UIState.LeftPanelWidth, UiState.ScreenMiddleX);
    }

    private bool TryGetCurrentStep(out MemTableStep step)
    {
        if (Steps.Count != 0)
        {
            step = Steps[UiState.CurrentStepIndex];
            return true;
        }
        
        step = null!;
        return false;
    }

    private void UpdateLayout(int leftPanelWidth, int screenMiddleX)
    {
        var rawLayout = Tree.GetLayout();
        Layout = rawLayout.OffsetLayout(leftPanelWidth, screenMiddleX);
    }
}