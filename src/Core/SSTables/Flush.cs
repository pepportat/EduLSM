using Core.Common;
using Core.SSTables.IoHelpers;
using Core.SSTables.IoOperations;
using Core.SSTables.Structure;

namespace Core.SSTables;

public static class Flush
{
    /// <summary>
    /// Writes the memTable to a file
    /// </summary>
    /// <param name="memTable">Memtable to be saved</param>
    /// <param name="directoryPath">Directory Path</param>
    /// <param name="falsePositiveRate">FalsePositivity rate of the bloom filter</param>
    public static SsTable FlushMemTable(IEnumerable<Kvp> memTable, string directoryPath, double falsePositiveRate = 0.01)
    {
        var memTableList = memTable.ToList();
        var keys = memTableList.Select(x => x.Key);

        var bloomFilter = BloomFilterBuilder.Create(keys, memTableList.Count, falsePositiveRate);
        var tableMetaData = new MetaData();
        tableMetaData.TotalRecordCount = memTableList.Count;
        tableMetaData.BlockCount = (int)Math.Ceiling(memTableList.Count / 10f);

        using var stream = File.Open(GetFileName(directoryPath), FileMode.Create);
        using var writer = new BinaryWriter(stream);
        
        tableMetaData.DataBlockOffset = writer.BaseStream.Position;
        var sparseIndex = FileWriter.WriteDataBlock(writer, memTableList).ToList();

        tableMetaData.SparseIndexOffset = writer.BaseStream.Position;
        FileWriter.WriteSparseIndex(writer, sparseIndex);
                
        tableMetaData.BloomFilterOffset =  writer.BaseStream.Position;
        FileWriter.WriteBloomFilter(writer, bloomFilter);
                
        FileWriter.WriteFooter(writer, tableMetaData);

        return new SsTable
        {
            KvpList = memTableList,
            Index = new SparseIndex(sparseIndex),
            BloomFilter = bloomFilter,
            Footer = tableMetaData
        };
    }

    private static string GetFileName(string directoryPath)
    {
        return $@"{directoryPath}\t1_{FileConstants.FileBaseName}_{DateTime.Now:yyyyMMddHHmmss}";
    }
}