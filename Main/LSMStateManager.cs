using Core.MemTables;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;

namespace Main;

public sealed class LsmStateManager
{
    public LsmStateManager()
    {
        Tree = new RedBlackTree();
        Layout = Tree.GetLayout(0,0, 0);
    }

    public IMemTable Tree { get; set;}
    public Dictionary<int, NodeSnapshot> Layout { get; set; }
    
    public void UpdateLayout(int yNodeSeparator, int screenMiddleX, int leftPanelWidth)
    {
        Layout = Tree.GetLayout(yNodeSeparator, screenMiddleX, leftPanelWidth);
    }
    
}