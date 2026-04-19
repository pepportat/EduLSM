using Core.SSTables.IoOperations;
using Core.SSTables.Structure;

namespace Core.SSTables.VisualizerHelpers;

public static class ReadAllSsTables
{
    public static List<SsTable> ReadAllTables(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath);
        var ssTables = new List<SsTable>();
        foreach (var file in files)
        {
            using (var stream = File.OpenRead(file))
            {
                var table = new SsTable();
                using var reader = new BinaryReader(stream);

                var footer = FileReader.ReadFooter(reader);
                table.Footer = footer;
                
                reader.BaseStream.Position = 0;
                table.KvpList = FileReader.ReadDataBlock(reader, footer.SparseIndexOffset);
                
                table.Index = FileReader.ReadSparseIndex(reader, footer.BloomFilterOffset);
                
                reader.BaseStream.Position = footer.BloomFilterOffset;
                table.BloomFilter = FileReader.ReadBloomFilter(reader);

                ssTables.Add(table);
            }
        }

        return ssTables;
    }
}