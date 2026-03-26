using System.Collections;
using Core.SSTables.Structure;

namespace Core.SSTables.IoHelpers;

public static class BloomFilterBuilder
{
    public static BloomFilter Create(IEnumerable<int> keys, int expectedItems, double falsePositiveRate = 0.01)
    {
        var size = OptimalBitCount(expectedItems, falsePositiveRate);
        var hashCount = OptimalHashCount(expectedItems, size);
        var bits = new BitArray(size);

        foreach (var key in keys)
            Add(bits, hashCount, size, key);
        return new  BloomFilter(bits, hashCount);
    }
    
    /// <summary>
    /// Serializes the bloom filter BitArray, HashCount and array size as an array of bytes
    /// </summary>
    /// <param name="filter">BloomFilter to be serialized</param>
    /// <returns>An array of bytes containing the size, hash count and bit array </returns>
    public static byte[] Serialize(BloomFilter filter)
    {
        var bytes = new byte[(filter.Size + 7) / 8 + 8];

        BitConverter.GetBytes(filter.Size).CopyTo(bytes, 0);
        BitConverter.GetBytes(filter.HashCount).CopyTo(bytes, 4);

        filter.Bits.CopyTo(bytes, 8);

        return bytes;
    }

    public static BloomFilter Deserialize(byte[] bytes)
    {
        var size = BitConverter.ToInt32(bytes, 0);
        var hashCount = BitConverter.ToInt32(bytes, 4);
        var bitBytes = bytes[8..];
        var bits = new BitArray(bitBytes) { Length = size };
        return new BloomFilter(bits, hashCount);
    }


    #region Private Methods

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
    /// Adds a new key to the Bloom Filter
    /// </summary>
    /// <param name="bits">BitArray</param>
    /// <param name="hashCount">Number of hashes</param>
    /// <param name="size">Size of BitArray</param>
    /// <param name="key">Key to be added</param>
    private static void Add(BitArray bits, int hashCount, int size, int key)
    {
        foreach (int pos in BloomFilterHashing.GetPositions(key, hashCount, size))
            bits[pos] = true;
    }

    #endregion
}

public static class BloomFilterHashing
{
    /// <summary>
    ///  Simulates K hash functions
    /// </summary>
    /// <param name="key">Key to be hashed</param>
    /// <param name="hashCount">Number of hashes</param>
    /// <param name="size">Size of BitArray</param>
    /// <returns>Collection of K positions, where K = optimal number of hash functions</returns>
    public static IEnumerable<int> GetPositions(int key, int hashCount, int size)
    {
        byte[] bytes = BitConverter.GetBytes(key);
        uint h1 = MurmurHash3(bytes, 0);
        uint h2 = MurmurHash3(bytes, h1);

        for (int i = 0; i < hashCount; i++)
            yield return (int)((h1 + (uint)i * h2) % (uint)size);
    }

    /// <summary>
    /// Simplified version of MurmurHash3 algorithm
    /// </summary>
    /// <param name="data">Data to be hashed</param>
    /// <param name="seed">Hashing seed</param>
    /// <returns>Hashed value of the data</returns>
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
}