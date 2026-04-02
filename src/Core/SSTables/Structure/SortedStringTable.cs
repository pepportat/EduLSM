using Core.Common;

namespace Core.SSTables.Structure;

public class SortedStringTable
{
    /// <summary>
    /// The Key-Value-DeletionMarker entries of a DataBlock
    /// </summary>
    IEnumerable<Kvp> KvpList { get; }
    
}
