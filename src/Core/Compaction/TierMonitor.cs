using Core.Common;
using static Core.Common.FileNameHelpers;

namespace Core.Compaction;

public static class TierMonitor
{
    public static List<int> NeedsCompaction(string directoryPath)
    {
        var tiers = new List<int>();
        var files = Directory.EnumerateFiles(directoryPath, $"*{FileConstants.FileBaseName}*", SearchOption.AllDirectories);
        var chunks = files.ToLookup(f => GetSsTableFileTier(Path.GetFileName(f)));

        var tier1Count = chunks[1].Count();
        var tier2Count = chunks[2].Count();

        if (tier1Count >= 3)
        {
            tiers.Add(1);
            tier2Count += 1;
        }

        if (tier2Count >= 3)
        {
            tiers.Add(2);
        }
        
        return tiers;
    }
}