using Core.Common;
using Core.SSTables.IoHelpers;
using Core.SSTables.IoOperations;
using Core.SSTables.Structure;

namespace Core.SSTables;

public static class Search
{
    /// <summary>
    /// Searches for the kvp associated with the key in the Data Files
    /// </summary>
    /// <param name="key">Search key</param>
    /// <param name="directoryPath">Directory paths</param>
    public static IEnumerable<SearchResult> SearchKey(int key, string directoryPath)
    {
        var filePaths = Directory.GetFiles(directoryPath);
        int numberOfFiles = filePaths.Length;
        var results = new List<SearchResult>();

        if (numberOfFiles == 0)
        {
            return results;
        }
        
        for (int i = 0; i < numberOfFiles; i++)
        {
            var result = new SearchResult(filePaths[i]);
            
            using var stream = File.OpenRead(filePaths[i]);
            using var reader = new BinaryReader(stream);
            
            var footer = FileReader.ReadFooter(reader);

            reader.BaseStream.Position = footer.BloomFilterOffset;

            var bloomFilter = FileReader.ReadBloomFilter(reader);
            
            var bfMightContain = bloomFilter.MightContain(key);

            if (!bfMightContain)
            {
                results.Add(result);
                continue;
            }
            result.FoundInBloomFilter = true;
            
            reader.BaseStream.Position = footer.SparseIndexOffset;
            var sparseIndex = FileReader.ReadSparseIndex(reader, footer.BloomFilterOffset);

            var siMightContain = sparseIndex.MightContain(key);
            if (!siMightContain)
            {
                results.Add(result);
                continue;
            }
            
            var offsetRange = sparseIndex.FindPossibleOffsetRange(key);
            
            if (offsetRange.start == offsetRange.end)
            {
                offsetRange.end = footer.SparseIndexOffset;
            }
            
            reader.BaseStream.Position = offsetRange.start;
            var dataBlocks = FileReader.ReadDataBlock(reader, offsetRange.end).ToList();

            result.FoundInSparseIndex = true;
            result.SparseIndexKey = dataBlocks.First().Key;
            
            result.KeyValuePair = dataBlocks.FirstOrDefault(kvp => kvp.Key == key);
            results.Add(result);
        }

        return results;
    }
}

public class SearchResult
{
    public string FileName { get; set; }
    public bool FoundInBloomFilter { get; set; } = false;
    public bool FoundInSparseIndex { get; set; } = false;
    public int? SparseIndexKey { get; set; } = null;
    public Kvp? KeyValuePair { get; set; } = null;

    public SearchResult(string filename)
    {
        FileName = filename;
    }
}