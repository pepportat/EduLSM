using System.Collections;
using System.Text;
using Core.SSTables.IoHelpers;

namespace Core.SSTables.Structure;

public class BloomFilter(BitArray bits, int hashCount, int size)
{
    public BitArray Bits { get; } = bits;
    public int Size { get; } = size;
    public int HashCount { get; } = hashCount;

    /// <summary>
    /// Checks if the bloom filter contains a specific key
    /// </summary>
    /// <param name="key">Key to be checked</param>
    /// <returns>A boolean value indicating if a key could or is definitely not in the bitarray</returns>
    public bool MightContain(int key)
    {
        return BloomFilterHashing.GetPositions(key, HashCount, Size).All(position => Bits[position]);
    }

    public override string ToString()
    {
        var sb = new StringBuilder("", Size);
        
        foreach (var bit in Bits)
        {
            sb.Append((bool)bit ? 1 : 0);
        }
        
        return sb.ToString();
    }
}