using System.Numerics;
using Core.MemTables.RedBlackTree.VisualizerHelpers;

namespace Core.MemTables.RedBlackTree;

public class RedBlackTree : IMemTable
{
    private readonly RedBlackNode _nil;
    private RedBlackNode _root;

    public int Count { get; private set; }

    public RedBlackTree()
    {
        _nil = RedBlackNode.CreateNil();
        _nil.Left =  _nil;
        _nil.Right = _nil;
        _nil.Parent = _nil;
        
        _root = _nil;
    }
    
    private bool IsNil(RedBlackNode node) => node == _nil;
    
    public (bool result, List<MemTableStep> steps) Add(int key, string value)
    {
        var steps = new List<MemTableStep> {new(StepKind.InsertStart, $"Begin insert for [{key}]")};

        AddInternal(key, value, steps);
        
        return (true, steps);
    }
    
    public (bool result, List<MemTableStep> steps) Remove(int key)
    {
        List<MemTableStep> steps = [new(StepKind.SearchStart, $"Begin remove for [{key}]")];

        var searchNode = FindNode(key, steps);

        //key not found
        if (IsNil(searchNode))
        {
            steps.Add(new MemTableStep(StepKind.DeleteNotFound, $"[{key}] not found"));
            steps.Add(new MemTableStep(StepKind.InsertStart, $"Begin insert for [{key}]"));
            AddInternal(key, "", steps, true);
            return (false, steps);
        }

        //already deleted
        if (searchNode.IsTombstone)
        {
            steps.Add(new MemTableStep(StepKind.DeleteAlreadyTombstoned, $"[{key}] already tombstoned"));
            return (true, steps);
        }
        
        //mark as tombstone
        steps.Add(new MemTableStep(StepKind.DeleteTombstone, $"Mark [{key}] as tombstone", searchNode.Key));
        searchNode.IsTombstone = true;
        Count--;
        return (true, steps);
    }

    public (string? value, List<MemTableStep> steps) Get(int key)
    {
        List<MemTableStep> steps = [new(StepKind.SearchStart, $"Begin search for [{key}]")];
        
        var sNode = FindNode(key, steps);

        if (IsNil(sNode))
        {
            steps.Add(new MemTableStep(StepKind.SearchMiss, $"[{key}] not found"));
            return (null, steps);
        }
        
        if (!sNode.IsTombstone)
        {
            steps.Add(new MemTableStep(StepKind.SearchHit, $"[{key}] found - value: {sNode.Value}", sNode.Key));
            return (sNode.Value, steps);
        }

        steps.Add(new MemTableStep(StepKind.SearchHit, $"[{key}] found as tombstone - value: {sNode.Value}"));
        return (null, steps);
    }

    public IEnumerable<(int, string)> GetSorted()
    {
        var sortedList = new List<(int, string)>(capacity: Count);

        InOrder(_root, sortedList);

        return sortedList;
    }

    // public List<NodeSnapshot> GetTreeSnapsShot()
    // {
    //     var list = new List<NodeSnapshot>();
    //     GetTreeSnapsShotInternal(_root, list, 0, -1f, 1f);
    //     return list;
    // }

    public Dictionary<int, NodeSnapshot> GetLayout(int yNodeSeparator, int screenMiddleX, int leftPanelWidth)
    {
        if (_root.IsNil) return new Dictionary<int, NodeSnapshot>();

        const double siblingDistance = 80.0;
        double levelDistance = yNodeSeparator;

        var nodes = new Dictionary<RedBlackNode, NodeSnapshot>();
        var result = new Dictionary<int, NodeSnapshot>();
        int inOrderIndex;

        CalculatePrelim(_root, 0);

        // offset should not be in this class
        float offset = leftPanelWidth / 2f + screenMiddleX - nodes[_root].Position.X;
        foreach (var kvp in nodes)
        {
            var pos = new Vector2(kvp.Value.Position.X + offset, kvp.Value.Position.Y + yNodeSeparator);
            
            result[kvp.Key.Key] = kvp.Value with { Key = kvp.Key.Key, Position = pos };
        }
        return result;

        void CalculatePrelim(RedBlackNode node, int depth)
        {
            if (node.IsNil) return;

            nodes[node] = new NodeSnapshot(
                node.Key,
                node.Value,
                node.Color,
                new Vector2(0, depth * (float)levelDistance),
                node.IsTombstone,
                node.Parent.Key
            );

            CalculatePrelim(node.Left, depth + 1);

            if (node.Left.IsNil && node.Right.IsNil)
            {
                var pos = new Vector2(inOrderIndex++ * (float)siblingDistance, nodes[node].Position.Y);
                nodes[node] = new NodeSnapshot(node.Key, node.Value, node.Color, pos, node.IsTombstone, node.Parent.Key);
                return;
            }

            CalculatePrelim(node.Right, depth + 1);

            double x;
            if (node.Left.IsNil)
            {
                x = nodes[node.Right].Position.X - siblingDistance / 2.0;
            }
            else if (node.Right.IsNil)
            {
                x = nodes[node.Left].Position.X + siblingDistance / 2.0;
            }
            else
            {
                double leftPos = nodes[node.Left].Position.X;
                double rightPos = nodes[node.Right].Position.X;

                if (rightPos - leftPos < siblingDistance)
                {
                    double shift = siblingDistance - (rightPos - leftPos);
                    ShiftSubtree(node.Right, shift);
                    rightPos += shift;
                }

                // Check deeper levels for overlap
                double minGap = GetContourMinGap(node.Left, node.Right);
                if (minGap < siblingDistance)
                {
                    double shift = siblingDistance - minGap;
                    ShiftSubtree(node.Right, shift);
                    rightPos += shift;
                }

                x = (leftPos + rightPos) / 2.0;
            }
            
            nodes[node] = new NodeSnapshot(node.Key, node.Value, node.Color, new Vector2((float)x, nodes[node].Position.Y), node.IsTombstone, node.Parent.Key);
        }

        void ShiftSubtree(RedBlackNode node, double shift)
        {
            var stack = new Stack<RedBlackNode>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.IsNil) continue;

                // was nodes[node] — should be nodes[current]
                nodes[current] = nodes[current] with { Position = new Vector2((float)(nodes[current].Position.X + shift), nodes[current].Position.Y) };

                stack.Push(current.Left);
                stack.Push(current.Right);
            }
        }

        double GetContourMinGap(RedBlackNode left, RedBlackNode right)
        {
            double minGap = double.MaxValue;
            var leftLevel = new List<RedBlackNode> { left };
            var rightLevel = new List<RedBlackNode> { right };

            while (leftLevel.Count > 0 && rightLevel.Count > 0)
            {
                var leftNodes = leftLevel.Where(n => !n.IsNil).ToList();
                var rightNodes = rightLevel.Where(n => !n.IsNil).ToList();

                if (leftNodes.Count == 0 || rightNodes.Count == 0) break;

                double leftMax = leftNodes.Max(n => nodes[n].Position.X);
                double rightMin = rightNodes.Min(n => nodes[n].Position.X);
                double gap = rightMin - leftMax;
                if (gap < minGap) minGap = gap;

                leftLevel = leftNodes.SelectMany(n => new[] { n.Left, n.Right }).ToList();
                rightLevel = rightNodes.SelectMany(n => new[] { n.Left, n.Right }).ToList();
            }

            return minGap == double.MaxValue ? siblingDistance : minGap;
        }
    }



    private RedBlackNode AddInternal(int key, string value, List<MemTableStep> steps, bool tombstone = false)
    {
        var searchNode = _root;
        var parent = _nil;
        
        while (!IsNil(searchNode))
        {
            var comparison = key.CompareTo(searchNode.Key);

            if (comparison == 0)
            {
                //Was deleted previously, so we need to update the item
                if (searchNode.IsTombstone)
                {
                    steps.Add(new MemTableStep(StepKind.InsertDuplicateUpdate, $"[{key}] tombstone cleared"));
                    searchNode.IsTombstone = false;
                    Count++;
                }
                else
                {
                    steps.Add(new MemTableStep(StepKind.InsertDuplicateUpdate, $"[{key}] updated in place"));
                }

                searchNode.Value = value;

                return searchNode;
            }
        }

        var newNode = new RedBlackNode
        (
            left: _nil,
            right: _nil,
            parent: parent,
            color: NodeColor.Red,
            key: key,
            value: value,
            isTombstone: tombstone
        );

        //First insert
        if (IsNil(parent))
        {
            _root = newNode;
        }
        else if (key.CompareTo(parent.Key) < 0)
        {
            parent.Left = newNode;
        }
        else
        {
            parent.Right = newNode;
        }
        
        InsertFixup(newNode);
        Count++;
        return newNode;
    }
    
    private void InOrder(RedBlackNode node, List<(int, string)> list)
    {
        if (IsNil(node))
        {
            return;
        }
        
        InOrder(node.Left, list);

        if (!node.IsTombstone)
        {
            list.Add((node.Key, node.Value));
        }
        
        InOrder(node.Right, list);
    }

    private void InsertFixup(RedBlackNode node)
    {
        while (node.Parent.Color == NodeColor.Red)
        {
            bool pLeft = node.Parent == node.Parent.Parent.Left;
            var uncle  = pLeft ? node.Parent.Parent.Right : node.Parent.Parent.Left;
            if (uncle.Color == NodeColor.Red)
            {
                node.Parent.Color = NodeColor.Black;
                uncle.Color = NodeColor.Black;
                node.Parent.Parent.Color = NodeColor.Red;
                node = node.Parent.Parent;
            }
            else
            {
                if (pLeft)
                {
                    if (node == node.Parent.Right)
                    {
                        node = node.Parent;
                        RotateLeft(node);
                    }
                    node.Parent.Color = NodeColor.Black;
                    node.Parent.Parent.Color = NodeColor.Red;
                    RotateRight(node.Parent.Parent);
                }
                else
                {
                    if (node == node.Parent.Left)
                    {
                        node = node.Parent;
                        RotateRight(node);
                    }
                    node.Parent.Color = NodeColor.Black;
                    node.Parent.Parent.Color = NodeColor.Red;
                    RotateLeft(node.Parent.Parent);
                }
            }
        }
        _root.Color = NodeColor.Black;
    }
    
    private void RotateLeft(RedBlackNode node)
    {
        var right = node.Right;
        node.Right = right.Left;
        if (!IsNil(right.Left)) right.Left.Parent = node;
        right.Parent = node.Parent;
        if (IsNil(node.Parent))
        {
            _root = right;
        }
        else if (node == node.Parent.Left)
        {
            node.Parent.Left = right;
        }
        else
        {
            node.Parent.Right = right;
        }

        right.Left = node;
        node.Parent = right;
    }

    private void RotateRight(RedBlackNode node)
    {
        var left = node.Left;
        node.Left = left.Right;
        if (!IsNil(left.Right))
        {
            left.Right.Parent = node;
        }

        left.Parent = node.Parent;
        if (IsNil(node.Parent))
        {
            _root = left;
        }
        else if (node == node.Parent.Right)
        {
            node.Parent.Right = left;
        }
        else
        {
            node.Parent.Left = left;
        }

        left.Right = node;
        node.Parent = left;
    }
    
    private RedBlackNode FindNode(int key, List<MemTableStep> steps)
    {
        var searchNode = _root;
        while (!IsNil(searchNode))
        {
            var comparison = key.CompareTo(searchNode.Key);

            if (comparison == 0)
            {
                return searchNode;
            }

            if (comparison < 0)
            {
                steps.Add(new MemTableStep(StepKind.SearchCompare, $"{key} < {searchNode.Key} -> Go left", searchNode.Key));
                searchNode = searchNode.Left;
            }
            else
            {
                steps.Add(new MemTableStep(StepKind.SearchCompare, $"{key} > {searchNode.Key} -> Go right", searchNode.Key));
                searchNode = searchNode.Right;
            }
        }

        return _nil;
    }
}