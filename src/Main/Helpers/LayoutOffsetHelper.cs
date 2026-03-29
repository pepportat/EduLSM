using Core.MemTables.RedBlackTree.VisualizerHelpers;

namespace Main.Helpers;

public static class LayoutOffsetHelper
{
    public static Dictionary<int, NodeSnapshot> OffsetLayout(this Dictionary<int, NodeSnapshot> layout, int leftPanelWidth, int screenMiddleX)
    {
        float rootX = layout.FirstOrDefault().Value?.Position.X ?? 0;
        float offset = leftPanelWidth / 2f + screenMiddleX - rootX;

        foreach (var kv in layout)
        {
            layout[kv.Key] = kv.Value with { Position = kv.Value.Position with { X = kv.Value.Position.X + offset } };
        }

        return layout;
    }
}