namespace Core.Compaction;

public static class TierMonitor
{
    public static List<int> NeedsCompaction(string directoryPath)
    {
        var tiers = new List<int>();
        var files = Directory.EnumerateFiles(directoryPath, "*edu_lsm_sstable*", SearchOption.AllDirectories);
        var chunks = files.ToLookup(f =>
        {
            if (f.StartsWith("t1"))
            {
                return 1;
            }

            if (f.StartsWith("t2"))
            {
                return 2;
            }

            return 3;
        });

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