using Core.Common;
using Core.SSTables.IoOperations;
using Core.SSTables.Structure;

namespace Core.SSTables.IoHelpers;

public class DataBlockIterator : IDisposable
{
    private string FilePath { get; set; }

    private long IteratingOffset { get; set; }
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

        IteratingOffset = Reader.ReadInt64();
        EndOffset = Reader.ReadInt64();

        Reader.BaseStream.Position = IteratingOffset;
    }

    /// <summary>
    /// Iterates through the Sstable
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool Next(out Kvp? result)
    {
        if (IteratingOffset >= EndOffset)
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