using Bogus;
using Core.Common;
using Core.MemTables;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Core.SSTables;
using Main.Helpers;
using Raylib_cs;
using static Core.SSTables.VisualizerHelpers.ReadAllSsTables;
using static Core.SSTables.Search;

namespace Main.UIHandlers;

public partial class LsmEngine
{
    private UIState UiState { get; set; }
    private IMemTable Tree { get; set;}
    private Dictionary<int, NodeSnapshot> Layout { get; set; }
    private List<MemTableStep> Steps { get; set; }
    private List<SearchResult> SsTablesSearchResults { get; set; }
    public Font Font { get; set; }
    private Faker Faker { get; set; }
    
    private readonly string _dataPath;
    
    public LsmEngine(UIState uiState, ProgramOptions programOptions)
    {
        UiState = uiState;
        Tree = new RedBlackTree();
        Layout = Tree.GetLayout();
        Steps = [];
        SsTablesSearchResults = [];
        _maxMemTableCount = programOptions.MaxMemTableCount;
        _dataPath = Path.Combine(programOptions.DataPath, FileConstants.DataDirectoryName);
        Faker = new Faker();
        
        
        if (!Directory.Exists(_dataPath))
        {
            Directory.CreateDirectory(_dataPath);
        }
        
        SsTables = ReadAllTables(_dataPath);
    }

    private void Search()
    {
        var key = int.Parse(UiState.Input);
        var (_, list) = Tree.Get(key);
        Steps = list;
        SsTablesSearchResults = SearchKey(key, _dataPath).ToList();
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