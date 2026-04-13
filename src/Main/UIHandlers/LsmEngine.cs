using Core.MemTables;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Main.Helpers;
using Raylib_cs;

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

    private void UpdateLayout()
    {
        Layout = Tree.GetLayout();
    }
}