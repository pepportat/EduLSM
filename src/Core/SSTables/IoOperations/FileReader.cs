using System.Collections;
using Core.Common;
using Core.SSTables.Structure;

namespace Core.SSTables.IoOperations;

public static class FileReader
{
    /// <summary>
    /// Reads the SsTable from file
    /// </summary>
    /// <param name="reader">BinaryReader object</param>
    /// <param name="stopOffset">Offset where data block ends in the file</param>
    /// <returns>The collection of Key-values</returns>
    public static IEnumerable<Kvp> ReadDataBlock(BinaryReader reader, long stopOffset)
    {
        var sortedStringTable = new List<Kvp>();

        while (reader.BaseStream.Position < stopOffset)
        {
            var key = reader.ReadInt32();
            var value = reader.ReadString();
            var isTombstoned = reader.ReadBoolean();
            
            sortedStringTable.Add(new(key, value, isTombstoned));
        }

        return sortedStringTable;
    }

    /// <summary>
    /// Reads SparseIndex from file
    /// </summary>
    /// <param name="reader">BinaryReader Object</param>
    /// <param name="stopOffset">Offset where SparseIndex ends in the file</param>
    /// <returns></returns>
    public static SparseIndex ReadSparseIndex(BinaryReader reader, long stopOffset)
    {
        var index = new List<SparseIndexEntries>();

        while (reader.BaseStream.Position < stopOffset)
        {
            var key = reader.ReadInt32();
            var offset = reader.ReadInt64();
            index.Add(new(key, offset));
        }
        
        return new SparseIndex(index);
    }
    
    /// <summary>
    /// Reads BloomFilter
    /// </summary>
    /// <param name="reader">BinaryReader object</param>
    /// <returns>BloomFilter</returns>
    public static BloomFilter ReadBloomFilter(BinaryReader reader)
    {
        var size = reader.ReadInt32();
        var hashCount = reader.ReadInt32();
        
        var bloomFilter = reader.ReadBytes(size);
        var bits = new BitArray(bloomFilter) {Length =  size};
        
        return new BloomFilter(bits, hashCount, size);
    }
    
    /// <summary>
    /// Reads the metadata/footer information of a file
    /// </summary>
    /// <param name="reader">BinaryReader object</param>
    /// <returns>File metadata</returns>
    public static MetaData ReadFooter(BinaryReader reader)
    {
        reader.BaseStream.Seek( -MetaData.ByteLenght, SeekOrigin.End);

        return new MetaData {
            DataBlockOffset = reader.ReadInt64(),
            BloomFilterOffset = reader.ReadInt64(),
            SparseIndexOffset = reader.ReadInt64(),
            BlockCount = reader.ReadInt32(),
            TotalRecordCount = reader.ReadInt32()
        };
    }
}