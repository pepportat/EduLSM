using System.Collections;
using System.Text;
using Core.SSTables.IoHelpers;

namespace Core.SSTables.Structure;

public class BloomFilter
{
    public BitArray Bits { get; }
    public int Size { get; }
    public int HashCount { get; }

    public BloomFilter(BitArray bits, int hashCount)
    {
        Bits = bits;
        Size = bits.Length;
        HashCount = hashCount;
    }

    /// <summary>
    /// Checks if the bloom filter contains a specific key
    /// </summary>
    /// <param name="key">Key to be checked</param>
    /// <returns>A boolean value indicating if a key could or is definitely not in the bitarray</returns>
    public bool MightContain(int key)
    {
        return BloomFilterHashing.GetPositions(key, HashCount, Size).All(position => Bits[position]);
    }
}