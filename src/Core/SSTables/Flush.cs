using Core.Common;
using Core.SSTables.IoHelpers;
using Core.SSTables.IoOperations;
using Core.SSTables.Structure;

namespace Core.SSTables;

public class Flush
{
    /// <summary>
    /// Writes the memTable to a file
    /// </summary>
    /// <param name="memTable">Memtable to be saved</param>
    /// <param name="filePath">File Path</param>
    /// <param name="falsePositiveRate">FalsePositivity rate of the bloom filter</param>
    public static void FlushMemTable(IEnumerable<Kvp> memTable, string filePath, double falsePositiveRate = 0.01)
    {
        var memTableList = memTable.ToList();
        var keys = memTableList.Select(x => x.Key);

        var bloomFilter = BloomFilterBuilder.Create(keys, memTableList.Count, falsePositiveRate);
        var tableMetaData = new MetaData();

        using (var stream = File.Open(filePath, FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream))
            {
                tableMetaData.DataBlockOffset = writer.BaseStream.Position;
                var sparseIndex = FileWriter.WriteDataBlock(writer, memTableList);

                tableMetaData.SparseIndexOffset = writer.BaseStream.Position;
                FileWriter.WriteSparseIndex(writer, sparseIndex);
                
                tableMetaData.BloomFilterOffset =  writer.BaseStream.Position;
                FileWriter.WriteBloomFilter(writer, bloomFilter);
                
                FileWriter.WriteFooter(writer, tableMetaData);
            }
        }
    }
}