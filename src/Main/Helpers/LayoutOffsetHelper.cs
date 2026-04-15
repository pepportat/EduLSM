using Core.MemTables.RedBlackTree.VisualizerHelpers;

namespace Main.Helpers;

public static class LayoutOffsetHelper
{
    public static Dictionary<int, NodeSnapshot> OffsetLayout(this Dictionary<int, NodeSnapshot> layout, int leftPanelWidth, int screenMiddleX)
    {
        float offset = leftPanelWidth / 2f + screenMiddleX;

        var offsetLayout = new Dictionary<int, NodeSnapshot>();
        
        foreach (var kv in layout)
        {
            offsetLayout[kv.Key] = kv.Value with { Position = kv.Value.Position with { X = kv.Value.Position.X + offset } };
        }

        return offsetLayout;
    }
}