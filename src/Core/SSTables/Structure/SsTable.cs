using Core.Common;

namespace Core.SSTables.Structure;

public class SsTable
{
    public string FileName { get; set; } = null!;
    public IEnumerable<Kvp> KvpList { get; set; } = null!;
    public SparseIndex Index { get; set; } = null!;
    public BloomFilter BloomFilter { get; set; } = null!;
    public MetaData Footer { get; set; } = null!;
}