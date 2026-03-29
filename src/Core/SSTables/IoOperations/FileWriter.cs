using System.Collections;
using Core.SSTables.IoHelpers;
using Core.SSTables.Structure;

namespace Core.SSTables.IoOperations;

public static class FileWriter
{
    /// <summary>
    /// Writes the memtable to file
    /// </summary>
    /// <param name="writer">BinaryWriter object</param>
    /// <param name="memTable">Key-value structure to be written to the file</param>
    /// <returns>The sparse index for the memtable</returns>
    public static IEnumerable<(int, long)> WriteDataBlock(BinaryWriter writer,
        IEnumerable<(int key, string value, bool isTombstoned)> memTable)
    {
        var chunks = memTable.Chunk(10);
        var sparseIndex = new List<(int, long)>();
        foreach (var chunk in chunks)
        {
            var currentOffset = writer.BaseStream.Position;
            var firstKey = chunk[0].key;

            sparseIndex.Add((firstKey, currentOffset));

            for (int i = 0; i < chunk.Length; ++i)
            {
                writer.Write(chunk[i].key);
                writer.Write(chunk[i].value);
                writer.Write(chunk[i].isTombstoned);
            }
        }

        return sparseIndex;
    }
    
    /// <summary>
    /// Writes the sparse index to file
    /// </summary>
    /// <param name="writer">BinaryWriter object</param>
    /// <param name="sparseIndex">Sparse index to be written to the file</param>
    public static void WriteSparseIndex(BinaryWriter writer, IEnumerable<(int, long)> sparseIndex)
    {
        foreach (var (key, offset) in sparseIndex)
        {
            writer.Write(key);
            writer.Write(offset);
        }
    }
    
    /// <summary>
    /// Writes the BloomFilter to file
    /// </summary>
    /// <param name="writer">BinaryWriter object</param>
    /// <param name="bloomFilter">BloomFilter to be written</param>
    /// <returns>Number of bytes written in file</returns>
    public static int WriteBloomFilter(BinaryWriter writer, BloomFilter bloomFilter)
    {
        var bytes = BloomFilterBuilder.Serialize(bloomFilter);

        writer.Write(bytes);

        return bytes.Length;
    }

    /// <summary>
    /// Writes the metadata/footer information to the file
    /// </summary>
    /// <param name="writer">BinaryWriter object</param>
    /// <param name="footer">File metadata</param>
    public static void WriteFooter(BinaryWriter writer, MetaData footer)
    {
        // Write the actual values
        writer.Write(footer.DataBlockOffset); // 8 bytes
        writer.Write(footer.BloomFilterOffset); // 8 bytes
        writer.Write(footer.SparseIndexOffset); // 8 bytes
        writer.Write(footer.BlockCount); // 4 bytes
        writer.Write(footer.TotalRecordCount); // 4 bytes
    }
}