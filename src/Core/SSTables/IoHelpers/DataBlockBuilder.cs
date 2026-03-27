namespace Core.SSTables.IoHelpers;

/// <summary>
/// Class to partition an IEnumerable of Key-Value pairs into appropriately sized blocks
/// </summary>
public class DataBlockBuilder
{
    private readonly List<(int Key, string Value )> _blockEntries;

    private readonly int _maxBlockSize;

    public bool IsFull => _blockEntries.Count >= _maxBlockSize;

    public int FirstKey => _blockEntries[0].Key;

    public DataBlockBuilder(int maxBlockSize = 10)
    {
        _maxBlockSize = maxBlockSize;
        _blockEntries = new(maxBlockSize);
    }
    
    /// <summary>
    /// Adds Key-Value entries to the current block being constructed
    /// </summary>
    ///<param name="kvp">Key-Value Pair</param>
    public void Add(ValueTuple<int, string> kvp)
    {
        _blockEntries.Add(kvp);
    }

    /// <summary>
    /// Finishes the block-building process. Re-initializes class after call
    /// </summary>
    /// <returns>Returns the list of KVPs accumulated in the block</returns>
    public List<(int, string)> Flush()
    {
        var copy = new List<(int, string )>(_blockEntries);
        _blockEntries.Clear();
        return copy;
    }
}