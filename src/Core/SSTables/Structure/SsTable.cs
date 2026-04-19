using Core.Common;

namespace Core.SSTables.Structure;

public class SsTable
{
    public string FileName { get; set; }
    public IEnumerable<Kvp> KvpList { get; set; }
    public SparseIndex Index { get; set; }
    public BloomFilter BloomFilter { get; set; }
    public MetaData Footer { get; set; }

    public SsTable()
    {
        
    }
}