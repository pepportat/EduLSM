using Core.Common;
using Core.MemTables;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Main.Helpers;
using Raylib_cs;
using static Core.SSTables.VisualizerHelpers.ReadAllSsTables;

namespace Main.UIHandlers;

public partial class LsmEngine
{
    private UIState UiState { get; set; }
    private IMemTable Tree { get; set;}
    private Dictionary<int, NodeSnapshot> Layout { get; set; }
    private List<MemTableStep> Steps { get; set; }
    public Font Font { get; set; }
    
    private readonly string _dataPath;
    
    public LsmEngine(UIState uiState, ProgramOptions programOptions)
    {
        UiState = uiState;
        Tree = new RedBlackTree();
        Layout = Tree.GetLayout();
        Steps = [];
        _maxMemTableCount = programOptions.MaxMemTableCount;
        _dataPath = Path.Combine(programOptions.DataPath, FileConstants.DataDirectoryName);

        if (!Directory.Exists(_dataPath))
        {
            Directory.CreateDirectory(_dataPath);
        }
        
        SsTables = ReadAllTables(_dataPath);
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