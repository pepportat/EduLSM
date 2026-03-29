namespace Core.SSTables.Structure;

public class SortedStringTable
{
    /// <summary>
    /// The Key-Value-DeletionMarker entries of a DataBlock
    /// </summary>
    IEnumerable<Kvp> KvpList { get; }
    
}

public record Kvp(int Key, string Value, bool IsTombStoned);