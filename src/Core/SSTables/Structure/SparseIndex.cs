using System.Text;
using Core.Common;

namespace Core.SSTables.Structure;

public class SparseIndex(IEnumerable<SparseIndexEntries> entries)
{
    public IEnumerable<SparseIndexEntries> KeyOffsetPairs { get; set; } = entries;

    /// <summary>
    /// Finds the offset range where the searchKey might exist
    /// </summary>
    /// <param name="searchKey">Key to be searched</param>
    /// <returns>A pair of offsets indicating the start and end of the Datablock where the key could exist</returns>
    public (long start, long end) FindPossibleOffsetRange(int searchKey)
    {
        long start, end;

        start = KeyOffsetPairs.Where(x => x.Key <= searchKey).Max(x => x.Offset);

        if (searchKey >= KeyOffsetPairs.Last().Key)
        {
            end = KeyOffsetPairs.Last().Offset;
        }
        else
        {
            end = KeyOffsetPairs.Where(x => x.Key > searchKey).Min(x => x.Offset);
        }

        return (start, end);
    }

    /// <summary>
    /// Checks if the searchKey falls in the keyRange of the SparseIndex
    /// </summary>
    /// <param name="searchKey">Key to search</param>
    /// <returns>A boolean value depending on if the key is in the key range or not</returns>
    public bool MightContain(int searchKey)
    {
        if (searchKey < KeyOffsetPairs.First().Key)
        {
            return false;
        }

        return true;
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