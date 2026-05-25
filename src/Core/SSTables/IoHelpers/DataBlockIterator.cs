using Core.Common;
using Core.SSTables.IoOperations;
using Core.SSTables.Structure;

namespace Core.SSTables.IoHelpers;

public class DataBlockIterator : IDisposable
{
    private string FilePath { get; set; }

    private long StartOffset { get; set; }
    private long EndOffset { get; set; }
    private FileStream FileStream { get; set; }
    private BinaryReader Reader { get; set; }

    /// <summary>
    /// Initializes the DataBlockIterator class.
    /// </summary>
    /// <param name="filePath">Path for Sstable file</param>
    public DataBlockIterator(string filePath)
    {
        FilePath = filePath;
        FileStream = File.OpenRead(FilePath);
        Reader = new BinaryReader(FileStream);

        Reader.BaseStream.Seek(-MetaData.ByteLenght, SeekOrigin.End);

        StartOffset = Reader.ReadInt64();
        Reader.BaseStream.Position += 8;
        EndOffset = Reader.ReadInt64();

        Reader.BaseStream.Position = StartOffset;
    }

    /// <summary>
    /// Iterates through the SsTable DataBlock segment
    /// </summary>
    /// <param name="result">Out parameter</param>
    /// <returns>True if the end of the DataBlock segment has not been reached by the </returns>
    public bool Next(out Kvp? result)
    {
        if (Reader.BaseStream.Position >= EndOffset)
        {
            result = null;
            return false;
        }

        var key = Reader.ReadInt32();
        var value = Reader.ReadString();
        var isTombstone = Reader.ReadBoolean();

        result = new(key, value, isTombstone);
        return true;
    }

    public void Dispose()
    {
        FileStream.Dispose();
        Reader.Dispose();
    }
}