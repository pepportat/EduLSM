using Core.SSTables.IoHelpers;
using Core.SSTables.IoOperations;
using Core.SSTables.Structure;

namespace Core.Compaction;

public class Compact
{
    public void CompactFiles(string directoryPath)
    {
        // 1. Read all files
        var ssTables = LoadSsTables(directoryPath);
        var chunks = ssTables.ToLookup(f =>
        {
            if (f.FileName.StartsWith("t1"))
            {
                return 1;
            }

            if (f.FileName.StartsWith("t2"))
            {
                return 2;
            }

            return 3;
        });

        var l = chunks[1];

        // 2. Compact them


        // 3. Write to disk

        // 4. Delete old files 
    }

    private List<SsTable> LoadSsTables(string directoryPath)
    {
        var filePaths = Directory.GetFiles(directoryPath).Reverse().ToArray();
        var results = new List<SsTable>();
        foreach (var filePath in filePaths)
        {
            var result = new SsTable();

            result.FileName = Path.GetFileName(filePath);

            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);

            var footer = FileReader.ReadFooter(reader);
            result.Footer = footer;

            reader.BaseStream.Position = footer.DataBlockOffset;
            var fileInfo = new FileInfo(filePath);
            var data = FileReader.ReadDataBlock(reader, fileInfo.Length);

            results.Add(result);
        }

        return results;
    }

    /*
    private List<SsTable> IterateAndCompact(List<string> fileNames)
    {
        var filesSorted = fileNames.OrderBy(f => f).ToList();
        var fileIterator1 = new DataBlockIterator(filesSorted[1]);
        var fileIterator2 = new DataBlockIterator(filesSorted[2]);
        var fileIterator3 = new DataBlockIterator(filesSorted[3]);
        
        var next1
    }
    */
    
}