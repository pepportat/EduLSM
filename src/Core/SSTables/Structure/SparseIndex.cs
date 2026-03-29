using System.Text;

namespace Core.SSTables.Structure;

public class SparseIndex(IEnumerable<SparseIndexEntries> entries)
{
    public IEnumerable<SparseIndexEntries> KeyOffsetPairs { get; set; } = entries;

    public (long start, long end) FindPossibleOffset(int searchKey)
    {
        var start = KeyOffsetPairs.Where(x => x.Key <= searchKey).Max(x => x.Offset);
        var end =  KeyOffsetPairs.Where(x => x.Key >= searchKey).Min(x => x.Offset);
        
        return (start, end);
    }

    public override string ToString()
    {
        var sb = new StringBuilder("");

        foreach (var keyOffset in KeyOffsetPairs)
        {
            sb.Append(keyOffset.Key).Append(": ");
            sb.Append(keyOffset.Offset).Append('\n');
        }
        
        return sb.ToString();
    }
}

public record SparseIndexEntries(int Key, long Offset);