using System.Collections;
using System.Text;

namespace Core.SSTables.Structure;

public class BloomFilter
{
    public BitArray Bits { get; }
    public int Size { get; }
    private int HashCount { get; }

    public BloomFilter(int expectedItems, double falsePositiveRate = 0.01)
    {
        Size = OptimalBitCount(expectedItems, falsePositiveRate);
        HashCount = OptimalHashCount(expectedItems, Size);
        Bits = new BitArray(Size);
    }

    public BloomFilter(BitArray bits, int hashCount)
    {
        Bits = bits;
        Size = bits.Length;
        HashCount = hashCount;
    }

    /// <summary>
    /// Adds a new key to the Bloom Filter
    /// </summary>
    /// <param name="key">Key to be added</param>
    public void Add(int key)
    {
        foreach (int pos in GetPositions(key))
            Bits[pos] = true;
    }

    /// <summary>
    /// Checks if the bloom filter contains a specific key
    /// </summary>
    /// <param name="key">Key to be checked</param>
    /// <returns>A boolean value indicating if a key could or is definitely not in the bitarray</returns>
    public bool MightContain(int key)
    {
        return GetPositions(key).All(position => Bits[position]);
    }

    /// <summary>
    /// Serializes the bloom filter BitArray, HashCount and array size as an array of bytes
    /// </summary>
    /// <returns>An array of bytes containing the size, hash count and bit array </returns>
    public byte[] Serialize()
    {
        byte[] bytes = new byte[(Size + 7) / 8 + 8];

        BitConverter.GetBytes(Size).CopyTo(bytes, 0);
        BitConverter.GetBytes(HashCount).CopyTo(bytes, 4);

        Bits.CopyTo(bytes, 8);

        return bytes;
    }

    public static BloomFilter Deserialize(byte[] bytes)
    {
        int size = BitConverter.ToInt32(bytes, 0);
        int hashCount = BitConverter.ToInt32(bytes, 4);
        byte[] bitBytes = bytes[8..];
        var bits = new BitArray(bitBytes) { Length = size };
        return new BloomFilter(bits, hashCount);
    }

    #region Private methods

    /// <summary>
    /// Calculates the optimal BitArray size
    /// </summary>
    /// <param name="n">Expected number of elements</param>
    /// <param name="p">False positivity rate</param>
    /// <returns>The optimal size for a BitArray</returns>
    private static int OptimalBitCount(int n, double p) =>
        (int)(-n * Math.Log(p) / (Math.Log(2) * Math.Log(2)));

    /// <summary>
    /// Calculates the optimal hashing count
    /// </summary>
    /// <param name="n">Expected number of elements</param>
    /// <param name="m">BitArray size</param>
    /// <returns>The optimal number of hash functions</returns>
    private static int OptimalHashCount(int n, int m) =>
        (int)((double)m / n * Math.Log(2));

    /// <summary>
    /// Simplified version of MurmurHash3 algorithm
    /// </summary>
    /// <param name="data">Data to be hashed</param>
    /// <param name="seed">Hashing seed</param>
    /// <returns></returns>
    private static uint MurmurHash3(byte[] data, uint seed)
    {
        uint h = seed;
        for (int i = 0; i < data.Length; i++)
        {
            h ^= data[i];
            h *= 0x5bd1e995;
            h ^= h >> 15;
        }

        return h;
    }

    /// <summary>
    ///  Simulates K hash functions
    /// </summary>
    /// <param name="key">Key to be hashed</param>
    /// <returns>Collection of K positions, where K = optimal number of hash functions</returns>
    private IEnumerable<int> GetPositions(int key)
    {
        byte[] bytes = BitConverter.GetBytes(key);
        uint h1 = MurmurHash3(bytes, 0);
        uint h2 = MurmurHash3(bytes, h1);

        for (int i = 0; i < HashCount; i++)
            yield return (int)((h1 + (uint)i * h2) % (uint)Size);
    }

    #endregion
}