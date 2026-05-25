using Core.Common;
using Core.SSTables;
using Core.SSTables.IoHelpers;
using Core.SSTables.IoOperations;
using Core.SSTables.Structure;

namespace Core.Compaction;

public static class SsTableCompacter
{
    public static (SsTable compactedTable, Dictionary<int, string> visualizerDictionary) CompactFiles(string directoryPath, int tier)
    {
        var files = GetAllFilesInTier(directoryPath, tier).ToList();
        
        var compactedTableAndDictionary = CompactTier(files, directoryPath);
        
        //DeleteFiles(files);

        return compactedTableAndDictionary;
    }

    private static IEnumerable<string> GetAllFilesInTier(string directoryPath, int tier)
    {
        var files = Directory.EnumerateFiles(directoryPath, "*edu_lsm_sstable*", SearchOption.AllDirectories);
        var chunks = files.ToLookup(f =>
        {
            var fileName = Path.GetFileName(f);
            if (fileName.StartsWith("t1"))
            {
                return 1;
            }

            if (fileName.StartsWith("t2"))
            {
                return 2;
            }

            return 3;
        });
        
        return chunks[tier].Order().TakeLast(3);
    }
    
    private static (SsTable compactedTable, Dictionary<int, string> visualizerDictionary) CompactTier(List<string> fileNames, string directoryPath)
    {
        var nextTier = GetNextTier(fileNames.First());
   
        var file0 = fileNames[0];
        var file1 = fileNames[1];
        var file2 = fileNames[2];
        
        using var fileIterator0 = new DataBlockIterator(file0); // Oldest
        using var fileIterator1 = new DataBlockIterator(file1); // Middle
        using var fileIterator2 = new DataBlockIterator(file2); // Newest

        Kvp? next0, next1, next2;
        var notEmpty0 = fileIterator0.Next(out next0);
        var notEmpty1 = fileIterator1.Next(out next1);
        var notEmpty2 = fileIterator2.Next(out next2);

        var compactedList = new List<Kvp>();
        var visualizerDictionary = new Dictionary<int, string>();

        while (notEmpty0 || notEmpty1 || notEmpty2)
        {
            var minKey = int.MaxValue;
            if (notEmpty0 && next0!.Key < minKey) minKey = next0.Key;
            if (notEmpty1 && next1!.Key < minKey) minKey = next1.Key;
            if (notEmpty2 && next2!.Key < minKey) minKey = next2.Key;

            Kvp?  winningKvp = null;
            var keyOriginFile = "";
            
            if (notEmpty0 && next0!.Key == minKey)
            {
                winningKvp = next0;
                keyOriginFile = file0;
            }
            if (notEmpty1 && next1!.Key == minKey)
            {
                winningKvp = next1;
                keyOriginFile = file1;
            }
            if (notEmpty2 && next2!.Key == minKey)
            {
                winningKvp = next2;
                keyOriginFile = file2;
            }

            if (winningKvp != null && !winningKvp.IsTombStoned)
            {
                compactedList.Add(winningKvp);
                visualizerDictionary[winningKvp.Key] = keyOriginFile;
            }

            if (notEmpty0 && next0!.Key == minKey) notEmpty0 = fileIterator0.Next(out next0);
            if (notEmpty1 && next1!.Key == minKey) notEmpty1 = fileIterator1.Next(out next1);
            if (notEmpty2 && next2!.Key == minKey) notEmpty2 = fileIterator2.Next(out next2);
        }

        var ssTable = Flush.FlushMemTable(memTable: compactedList, directoryPath: directoryPath, tier: nextTier);
        
        return (ssTable, visualizerDictionary);
    }

    private static int GetNextTier(string fileName)
    {
        fileName = Path.GetFileName(fileName);
        
        return fileName[1] switch
        {
            '1' => 2,
            _ => 3
        };
    }
    
    private static void DeleteFiles(IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            File.Delete(fileName);
        }
    }
}
