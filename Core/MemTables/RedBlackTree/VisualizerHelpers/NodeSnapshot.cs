using System.Numerics;

namespace Core.MemTables.RedBlackTree.VisualizerHelpers;

public record NodeSnapshot(int Key, string Value, NodeColor Color, Vector2 Position, bool IsTombstone, int ParentKey);