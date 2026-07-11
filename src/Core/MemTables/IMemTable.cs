using Core.Common;
using Core.MemTables.RedBlackTree.VisualizerHelpers;

namespace Core.MemTables;

public interface IMemTable
{
    int Count { get; }
    List<MemTableStep> Add(int key, string value);
    List<MemTableStep> Remove(int key);
    List<MemTableStep> Get(int key);
    IEnumerable<Kvp> GetSorted();
    Dictionary<int, NodeSnapshot> GetLayout();
    void Clear();
}