using System.Numerics;
using Core.Common;
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
        var steps = new List<MemTableStep> {new(StepKind.InsertStart, $"Begin insert for [{key}]", null, GetLayout())};

        AddInternal(key, value, steps);
        
        return (true, steps);
    }
    
    public (bool result, List<MemTableStep> steps) Remove(int key)
    {
        List<MemTableStep> steps = [new(StepKind.DeleteStart, $"Begin remove for [{key}]", null, GetLayout())];

        var searchNode = FindNode(key, steps);

        //key not found
        if (IsNil(searchNode))
        {
            steps.Add(new MemTableStep(StepKind.DeleteNotFound, $"[{key}] not found", null, GetLayout()));
            steps.Add(new MemTableStep(StepKind.InsertStart, $"Begin insert for [{key}]", null, GetLayout()));
            AddInternal(key, "", steps, true);
            return (false, steps);
        }

        //already deleted
        if (searchNode.IsTombstone)
        {
            steps.Add(new MemTableStep(StepKind.DeleteAlreadyTombstoned, $"[{key}] already tombstoned", searchNode.Key, GetLayout()));
            return (true, steps);
        }
        
        //mark as tombstone
        searchNode.IsTombstone = true;
        steps.Add(new MemTableStep(StepKind.DeleteTombstone, $"Mark [{key}] as tombstone", searchNode.Key, GetLayout()));
        return (true, steps);
    }

    public (string? value, List<MemTableStep> steps) Get(int key)
    {
        List<MemTableStep> steps = [new(StepKind.SearchStart, $"Begin search for [{key}]", null, GetLayout())];
        
        var sNode = FindNode(key, steps);

        if (IsNil(sNode))
        {
            steps.Add(new MemTableStep(StepKind.SearchMiss, $"[{key}] not found", null, GetLayout()));
            return (null, steps);
        }
        
        if (!sNode.IsTombstone)
        {
            steps.Add(new MemTableStep(StepKind.SearchHit, $"[{key}] found - value: {sNode.Value}", sNode.Key, GetLayout()));
            return (sNode.Value, steps);
        }

        steps.Add(new MemTableStep(StepKind.SearchHit, $"[{key}] found as tombstone - value: {sNode.Value}", sNode.Key, GetLayout()));
        return (null, steps);
    }

    public IEnumerable<Kvp> GetSorted()
    {
        var sortedList = new List<Kvp>(capacity: Count);

        InOrder(_root, sortedList);

        return sortedList;
    }

    public Dictionary<int, NodeSnapshot> GetLayout()
    {
        if (_root.IsNil) return new Dictionary<int, NodeSnapshot>();

        const float siblingDistance = 30f;
        const float levelDistance = 80f;

        var xPos = new Dictionary<RedBlackNode, float>();
        var depths = new Dictionary<RedBlackNode, int>();
        var result = new Dictionary<int, NodeSnapshot>();
        int index = 0;

        AssignX(_root, 0);
        CenterParents(_root);

        float rootX = xPos[_root];
        foreach (var kvp in xPos)
        {
            var node = kvp.Key;
            result[node.Key] = new NodeSnapshot(
                node.Key,
                node.Value,
                node.Color,
                new Vector2(kvp.Value - rootX, (depths[node] + 1) * levelDistance),
                node.IsTombstone,
                node.Parent.Key
            );
        }

        return result;

        void AssignX(RedBlackNode node, int depth)
        {
            while (true)
            {
                if (node.IsNil) return;
                depths[node] = depth;
                AssignX(node.Left, depth + 1);
                xPos[node] = index++ * siblingDistance;
                node = node.Right;
                depth = depth + 1;
            }
        }

        void CenterParents(RedBlackNode node)
        {
            if (node.IsNil) return;
            CenterParents(node.Left);
            CenterParents(node.Right);

            if (node.Left.IsNil && node.Right.IsNil)
            {
                return;
            }

            if (node.Left.IsNil)
            {
                xPos[node] = xPos[node.Right] - siblingDistance / 2f;
            }
            else if (node.Right.IsNil)
            {
                xPos[node] = xPos[node.Left] + siblingDistance / 2f;
            }
            else
            {
                xPos[node] = (xPos[node.Left] + xPos[node.Right]) / 2f;
            }
        }
    }

    public void Clear()
    {
        Count = 0;
        _root = _nil;
    }

    private void AddInternal(int key, string value, List<MemTableStep> steps, bool tombstone = false)
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
                    searchNode.IsTombstone = false;
                    steps.Add(new MemTableStep(StepKind.InsertDuplicateUpdate, $"[{key}] tombstone cleared", key, GetLayout()));
                    Count++;
                }
                else
                {
                    steps.Add(new MemTableStep(StepKind.InsertDuplicateUpdate, $"[{key}] updated in place", key, GetLayout()));
                }
                
                searchNode.Value = value;
                return;
            }

            parent = searchNode;
            if (comparison > 0)
            {
                steps.Add(new MemTableStep(StepKind.InsertTraverse, $"{key} > {searchNode.Key} -> Go right", searchNode.Key, GetLayout()));
                searchNode = searchNode.Right;
            }
            else
            {
                steps.Add(new MemTableStep(StepKind.InsertTraverse, $"{key} < {searchNode.Key} -> Go left", searchNode.Key, GetLayout()));
                searchNode = searchNode.Left;
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
            steps.Add(new MemTableStep(StepKind.InsertAttach, $"Attach [{key}] to root", key, GetLayout()));
        }
        else if (key.CompareTo(parent.Key) < 0)
        {
            parent.Left = newNode;
            steps.Add(new MemTableStep(StepKind.InsertAttach, $"Attach [{key}] to left of [{parent.Key}]", key, GetLayout()));
        }
        else
        {
            parent.Right = newNode;
            steps.Add(new MemTableStep(StepKind.InsertAttach, $"Attach [{key}] to right of [{parent.Key}]", key, GetLayout()));
        }
        
        InsertFixup(newNode, steps);
        Count++;
    }
    
    private void InOrder(RedBlackNode node, List<Kvp> list)
    {
        if (IsNil(node))
        {
            return;
        }
        
        InOrder(node.Left, list);
        
        list.Add(new Kvp(node.Key, node.Value, node.IsTombstone));
        
        InOrder(node.Right, list);
    }

    private void InsertFixup(RedBlackNode node, List<MemTableStep> steps)
    {
        while (node.Parent.Color == NodeColor.Red)
        {
            bool pLeft = node.Parent == node.Parent.Parent.Left;
            var uncle  = pLeft ? node.Parent.Parent.Right : node.Parent.Parent.Left;
            
            if (uncle.Color == NodeColor.Red) //Case 1: uncle is RED
            {
                steps.Add(new MemTableStep(StepKind.FixupCase, $"Case 1{(pLeft ? "" : "m")}: Uncle RED — recolour parent/uncle BLACK, grandparent RED", node.Parent.Parent.Key, GetLayout()));
                
                node.Parent.Color = NodeColor.Black;
                steps.Add(new MemTableStep(StepKind.Recolour, $"Recolour parent [{node.Parent.Key}]:->BLACK", node.Parent.Key, GetLayout()));
                
                uncle.Color = NodeColor.Black;
                steps.Add(new MemTableStep(StepKind.Recolour, $"Recolour uncle [{uncle.Key}]:->BLACK", uncle.Key, GetLayout()));
                
                node.Parent.Parent.Color = NodeColor.Red;
                steps.Add(new MemTableStep(StepKind.Recolour, $"Recolour grandparent [{node.Parent.Parent.Key}]:->RED", node.Parent.Parent.Key, GetLayout()));
                
                node = node.Parent.Parent;
            }
            else //Case 2: uncle is BLACK
            {
                if (pLeft) // Case: Parent is the left child
                {
                    if (node == node.Parent.Right)
                    {
                        node = node.Parent;
                        steps.Add(new MemTableStep(StepKind.FixupCase, $"Case 2a: node is right child — left-rotate parent [{node.Key}]", node.Key, GetLayout()));
                        RotateLeft(node, steps);
                    }
                    
                    steps.Add(new MemTableStep(StepKind.FixupCase, $"Case 3a: Left-line — recolour + right-rotate grandparent [{node.Parent.Parent.Key}]", node.Parent.Parent.Key, GetLayout()));
                    
                    
                    
                    node.Parent.Color = NodeColor.Black;
                    steps.Add(new MemTableStep(StepKind.Recolour, $"Recolour parent [{node.Parent.Key}]:->BLACK", node.Parent.Key, GetLayout()));
                    
                    node.Parent.Parent.Color = NodeColor.Red;
                    steps.Add(new MemTableStep(StepKind.Recolour, $"Recolour grandparent [{node.Parent.Parent.Key}]:->RED", node.Parent.Parent.Key, GetLayout()));
                    
                    RotateRight(node.Parent.Parent, steps);
                }
                else // Case: Parent is the right child
                {
                    if (node == node.Parent.Left)
                    {
                        node = node.Parent;
                        steps.Add(new MemTableStep(StepKind.FixupCase, $"Case 2b: node is left child — right-rotate parent [{node.Key}]", node.Key, GetLayout()));
                        RotateRight(node, steps);
                    }
                    
                    steps.Add(new MemTableStep(StepKind.FixupCase, $"Case 3b: Right-line — recolour + left-rotate grandparent [{node.Parent.Parent.Key}]", node.Parent.Parent.Key, GetLayout()));
                    
                    node.Parent.Color = NodeColor.Black;
                    steps.Add(new MemTableStep(StepKind.Recolour, $"Recolour parent [{node.Parent.Key}]:->BLACK", node.Parent.Key, GetLayout()));
                    
                    node.Parent.Parent.Color = NodeColor.Red;
                    steps.Add(new MemTableStep(StepKind.Recolour, $"Recolour grandparent [{node.Parent.Parent.Key}]:->RED", node.Parent.Parent.Key, GetLayout()));
                    
                    RotateLeft(node.Parent.Parent, steps);
                }
            }
        }
        _root.Color = NodeColor.Black;
        steps.Add(new MemTableStep(StepKind.Recolour, "Recolour root:-> BLACK", _root.Key, GetLayout()));
    }
    
    private void RotateLeft(RedBlackNode node, List<MemTableStep> steps)
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
        steps.Add(new MemTableStep(StepKind.Rotation, $"LEFT-ROTATE on [{node.Key}]", node.Key, GetLayout()));
    }

    private void RotateRight(RedBlackNode node, List<MemTableStep> steps)
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
        steps.Add(new MemTableStep(StepKind.Rotation, $"RIGHT-ROTATE on [{node.Key}]", node.Key, GetLayout()));
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
                steps.Add(new MemTableStep(StepKind.SearchCompare, $"{key} < {searchNode.Key} -> Go left", searchNode.Key, GetLayout()));
                searchNode = searchNode.Left;
            }
            else
            {
                steps.Add(new MemTableStep(StepKind.SearchCompare, $"{key} > {searchNode.Key} -> Go right", searchNode.Key, GetLayout()));
                searchNode = searchNode.Right;
            }
        }

        return _nil;
    }
}