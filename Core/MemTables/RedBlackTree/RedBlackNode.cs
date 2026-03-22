namespace Core.MemTables.RedBlackTree;

public class RedBlackNode
{
    public RedBlackNode(
        bool isNil
    )
    {
        IsNil = isNil;
        Value = string.Empty;
        Key = -1;
        Color = NodeColor.Black;
        IsTombstone = false;
        Left = null!;
        Right = null!;
        Parent = null!;
    }
    
    public RedBlackNode(
        RedBlackNode left,
        RedBlackNode right,
        RedBlackNode parent,
        int key,
        string value,
        NodeColor color,
        bool isTombstone = false
    )
    {
        Left = left;
        Right = right;
        Parent = parent;
        Key = key;
        Value = value;
        Color = color;
        IsTombstone = isTombstone;
    }

    public static RedBlackNode CreateNil()
    {
        return new RedBlackNode(true);
    }
    
    public int Key { get; set; }
    public string Value { get; set; }
    public bool IsTombstone { get; set; }
    public NodeColor Color { get; set; }
    public bool IsNil { get; set; }
    

    public RedBlackNode Left { get; set; } = null!;
    public RedBlackNode Right { get; set; } = null!;
    public RedBlackNode Parent { get; set; } = null!;
}