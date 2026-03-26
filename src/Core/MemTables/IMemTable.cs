using Core.MemTables.RedBlackTree.VisualizerHelpers;

namespace Core.MemTables;

public interface IMemTable
{
    int Count { get; }
    (bool result, List<MemTableStep> steps) Add(int key, string value);
    (bool result, List<MemTableStep> steps) Remove(int key);
    (string? value, List<MemTableStep> steps) Get(int key);
    IEnumerable<(int key, string value)> GetSorted();
    Dictionary<int, NodeSnapshot> GetLayout();
}