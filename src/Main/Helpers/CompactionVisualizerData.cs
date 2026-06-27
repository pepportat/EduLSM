using Core.SSTables.Structure;
using Raylib_cs;
using static Main.Helpers.LinqHelper;

namespace Main.Helpers;

public class CompactionVisualizerData
{
    public CompactionVisualizerData(List<SsTable> compactedTables, SsTable compactionResult, Dictionary<int, string> keySources)
    {
        CompactedTables = compactedTables;
        CompactionResult = compactionResult;
        KeySources = keySources;
        SsTableColors = Choose(ColorList.Colors, 3).ToList();
    }

    public List<SsTable> CompactedTables { get; }
    public SsTable CompactionResult { get; }
    
    private Dictionary<int, string> KeySources { get; }
    private List<Color> SsTableColors { get; }
    
    
    public Color SsTableColor(SsTable ssTable) => SsTableColors[CompactedTables.IndexOf(ssTable)];

    public Color KeyColor(int key)
    {
        var keySource = Path.GetFileName(KeySources[key]);
        
        var sourceSsTable = CompactedTables.First(s => s.FileName == keySource);
        
        return SsTableColor(sourceSsTable);
    }
}