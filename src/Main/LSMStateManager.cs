using Core.MemTables;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Main.Helpers;

namespace Main;

public sealed class LsmStateManager
{
    public LsmStateManager()
    {
        Tree = new RedBlackTree();
        Layout = Tree.GetLayout();
    }

    public IMemTable Tree { get; set;}
    public Dictionary<int, NodeSnapshot> Layout { get; private set; }
    
    public void UpdateLayout(int leftPanelWidth, int screenMiddleX)
    {
        var rawLayout = Tree.GetLayout();
        Layout = rawLayout.OffsetLayout(leftPanelWidth, screenMiddleX);
    }
}