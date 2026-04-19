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
    public static IEnumerable<Kvp?> SearchKey(int key, string directoryPath)
    {
        var filePaths = Directory.GetFiles(directoryPath);
        int numberOfFiles = filePaths.Length;
        var kvps = new List<Kvp?>();

        for (int i = 0; i < numberOfFiles; i++)
        {
            using (var stream = File.OpenRead(filePaths[i]))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var footer = FileReader.ReadFooter(reader);

                    reader.BaseStream.Position = footer.BloomFilterOffset;

                    var bloomFilter = FileReader.ReadBloomFilter(reader);

                    var bfMightContain = bloomFilter.MightContain(key);

                    if (!bfMightContain)
                    {
                        continue;
                    }

                    reader.BaseStream.Position = footer.SparseIndexOffset;
                    var sparseIndex = FileReader.ReadSparseIndex(reader, footer.BloomFilterOffset);

                    var siMightContain = sparseIndex.MightContain(key);
                    if (!siMightContain)
                    {
                        continue;
                    }

                    var offsetRange = sparseIndex.FindPossibleOffsetRange(key);
                    reader.BaseStream.Position = offsetRange.start;
                    var dataBlocks = FileReader.ReadDataBlock(reader, offsetRange.end);

                    kvps.Add(dataBlocks.FirstOrDefault(kvp => kvp.Key == key));
                }
            }
        }

        return kvps;
    }
}