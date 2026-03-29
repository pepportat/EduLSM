using Core.MemTables.RedBlackTree.VisualizerHelpers;

namespace Core.SSTables.Structure;

public class MetaData
{
    /// <summary>
    /// Lenght of Metadata block in the file
    /// </summary>
    public const int ByteLenght = 32;
    
    /// <summary>
    /// Offset where the DataBlock starts
    /// </summary>
    public long DataBlockOffset { get; set; } = 0;
    
    /// <summary>
    /// Offset where the BloomFilter starts
    /// </summary>
    public long BloomFilterOffset { get; set; }
    
    /// <summary>
    /// Offset where the SparseIndex starts
    /// </summary>
    public long SparseIndexOffset { get; set; }
    
    /// <summary>
    /// Total BlockCount (Length of sparse index)
    /// </summary>
    public int BlockCount { get; set; }
    
    /// <summary>
    /// Total entries in the DataBlock
    /// </summary>
    public int TotalRecordCount { get; set; }
}