using System.Numerics;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Raylib_cs;
using static Main.Helpers.StepColorHelper;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;

namespace Main;

class Program
{
    private const int MaxInputChars = 5;
    private const int LeftPanelWidth = 400;
    
    private static void Main()
    {
        LsmStateManager manager = new LsmStateManager();
        List<MemTableStep> steps = [];
        //Dictionary<int, Vector2> nodeCords = [];
        

        var screenWidth = 1200;
        var screenHeight = 800;
        int screenMiddleX;
        const int yNodeSeparator = 80;
        string input = "";

        Camera2D treeCamera = new Camera2D
        {
            Zoom = 1.0f
        };

        Camera2D leftPanelCamera = new Camera2D
        {
            Zoom = 1.0f
        };


        SetConfigFlags(ConfigFlags.ResizableWindow);
        InitWindow(screenWidth, screenHeight, "Edu LSM");

        SetTargetFPS(60);
        
        while (!WindowShouldClose())
        {
            screenWidth = GetScreenWidth();
            screenHeight = GetScreenHeight();
            screenMiddleX = screenWidth / 2;
            
            if (IsMouseButtonDown(MouseButton.Left) && !CheckCollisionPointRec(GetMousePosition(), new Rectangle(0, 0, LeftPanelWidth, screenHeight)))
            {
                Vector2 delta = GetMouseDelta();
                
                delta = Vector2Scale(delta, -1.0f/treeCamera.Zoom);
                treeCamera.Target = Vector2Add(treeCamera.Target, delta);
            }
            
            float wheel = GetMouseWheelMove();
            if (wheel != 0)
            {
                if (CheckCollisionPointRec(GetMousePosition(), new Rectangle(0, 0, LeftPanelWidth, screenHeight)))
                {
                    leftPanelCamera.Offset.Y += wheel * 20;
                    
                    leftPanelCamera.Offset.Y = Clamp(leftPanelCamera.Offset.Y, -screenHeight, 0);
                }
                else
                {
                    Vector2 mouseWorldPos = GetScreenToWorld2D(GetMousePosition(), treeCamera);
                
                    treeCamera.Offset = GetMousePosition();
                
                    treeCamera.Target = mouseWorldPos;
                
                    float scale = 0.2f*wheel;
                    treeCamera.Zoom = Clamp((float)Math.Exp(Math.Log(treeCamera.Zoom)+scale), 0.125f, 10.0f);
                }
            }
            
            
            BeginDrawing();
                ClearBackground(new Color(3, 7, 18, 255));
            
                HandleInput();
                
                BeginMode2D(treeCamera);
                    Rlgl.PushMatrix();
                        Rlgl.Translatef(600, 1600, 0);
                        Rlgl.Rotatef(90, 1, 0, 0);
                        DrawGridCustom(200, 50, Color.DarkGray);
                    Rlgl.PopMatrix();
                    
                    DrawTree();
                    
                EndMode2D();

                BeginMode2D(leftPanelCamera);
                    DrawLeftPanel(LeftPanelWidth, 20, 4);
                EndMode2D();
            
            EndDrawing();
        }
        
        CloseWindow();

        return;
        
        void HandleInput()
        {
            int key = GetKeyPressed();
            char c = (char)key;
        
            // Check if more characters can be added
            if (c is >= '0' and <= '9' && (input.Length < MaxInputChars))
            {
                input += (char)key;
            }
        
            if (IsKeyDown(KeyboardKey.Backspace) && (input.Length > 0))
            {
                input = input[..^1];
            }
            
            if (IsKeyDown(KeyboardKey.Enter) && (input.Length > 0))
            {
                var (b, list) = manager.Tree.Add(int.Parse(input), $"Data for key {input}");
                manager.UpdateLayout(yNodeSeparator, screenMiddleX, LeftPanelWidth);
                input = "";
            }
            
            if (IsKeyDown(KeyboardKey.S) && (input.Length > 0))
            {
                var (b, list) = manager.Tree.Get(int.Parse(input));
                steps = list;
                input = "";
            }
            
            if (IsKeyDown(KeyboardKey.R) && (input.Length > 0))
            {
                var (b, list) = manager.Tree.Remove(int.Parse(input));
                steps = list;
                manager.UpdateLayout(yNodeSeparator, screenMiddleX, LeftPanelWidth);
                input = "";
            }
        }
        
        
        void DrawLeftPanel(int width, int fontSize = 16, int separatorHeight = 2)
        {
            DrawRectangle(0, 0, width, screenHeight * 2, Color.Black);
            DrawRectangleLines(0, 0, width, screenHeight * 2, Color.White);
            DrawText($"Count: {manager.Tree.Count}", 10, 10, fontSize, Color.White);
            DrawText($"Input: {input}", 10, 10 + separatorHeight + fontSize, fontSize, Color.White);

            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];

                int y = 10 + ((i + 2) * separatorHeight) + ((i + 2) * fontSize);
                
                DrawText(step.Description, 10, y, fontSize, Color.White);

                if (step.Key.HasValue)
                {
                    var mousePos = GetMousePosition();                                                                                                                                                                                                
                    var mouseWorldPos = GetScreenToWorld2D(mousePos, leftPanelCamera);
                    
                    if (CheckCollisionPointRec(mouseWorldPos, new Rectangle(10, y, width, fontSize + 2)))
                    {
                        var nodeCord = manager.Layout[step.Key.Value];

                        BeginMode2D(treeCamera);
                            DrawRing(new Vector2((int)nodeCord.Position.X, (int)nodeCord.Position.Y), 28, 34, 0, 360, 24, GetStepNodeColor(step.Kind));
                        EndMode2D();
                    }
                }
            }
        }
        
        

        
        
        void DrawTree()
        {
            foreach (var kv in manager.Layout.Where(node => node.Value.ParentKey != -1))
            {
                DrawEdges(kv.Value);
            }

            foreach (var kv in manager.Layout)
            {
                DrawNode(kv.Value, fontSize: 20);
            }
                    
            foreach (var kv in manager.Layout)
            {
                AddOnHoverForNodes(kv.Value, fontSize: 20);
            }
        }
        
        void DrawNode(NodeSnapshot node, int radius = 20, int fontSize = 10)
        {
            var nodeColor = node.Color == NodeColor.Black ? new Color( 30, 41, 59,255) : new Color(220, 38, 38,255);

            int circleX = (int)manager.Layout[node.Key].Position.X;
            int circleY = (int)manager.Layout[node.Key].Position.Y;
        
            DrawCircle(circleX, circleY, radius, nodeColor);
        
            int textWidth = MeasureText($"{node.Key}", fontSize);
        
            DrawText($"{node.Key}", circleX - textWidth / 2, circleY - fontSize / 2, fontSize, Color.White);

            if (node.IsTombstone)
            {
                DrawText("t", circleX + 8, circleY - 20, 15, Color.White);
            }
        }
        
        void DrawEdges(NodeSnapshot node)
        {
            var start = manager.Layout[node.Key].Position;
            var end = manager.Layout[node.ParentKey].Position;

            DrawLineEx(
                start,
                end,
                1.5f,
                Color.White
            );
        }
        
        void AddOnHoverForNodes(NodeSnapshot node, int radius = 20, int fontSize = 10)
        {
            int circleX = (int)manager.Layout[node.Key].Position.X;
            int circleY = (int)manager.Layout[node.Key].Position.Y;
                
            var mousePos = GetMousePosition();                                                                                                                                                                                                
            var mouseWorldPos = GetScreenToWorld2D(mousePos, treeCamera);  // Convert mouse to world coords                                                                                                                                       
            if (CheckCollisionPointRec(mouseWorldPos, new Rectangle(circleX - radius, circleY - radius, radius * 2, radius * 2)))                                                                                                             
            {                                                                                                                                                                                                                                 
                DrawCircleLines(circleX, circleY, radius, Color.White);                                                                                                                                                                       
                DrawText($"{node.Value}", circleX + 15, circleY - 30, fontSize, Color.White);                                                                                                                                                 
            }   
        }
    }

    
    private static void DrawGridCustom(int slices, float spacing, Color color)
    {
        int halfSlices = slices/2;

        Rlgl.Begin(DrawMode.Lines);
        for (var i = -halfSlices; i <= halfSlices; i++)
        {
            Rlgl.Color3f(color.R / 255f, color.G / 255f, color.B / 255f);
                

            Rlgl.Vertex3f(i*spacing, 0.0f, -halfSlices*spacing);
            Rlgl.Vertex3f(i*spacing, 0.0f, halfSlices*spacing);

            Rlgl.Vertex3f(-halfSlices*spacing, 0.0f, i*spacing);
            Rlgl.Vertex3f(halfSlices*spacing, 0.0f, i*spacing);
        }
        Rlgl.End();
    }

    // private static Dictionary<int, Vector2> SnapshotToCords(RedBlackNode root, int yNodeSeparator, int screenMiddleX,
    //     int leftPanelWidth)
    // {
    //     if (root.IsNil) return new Dictionary<int, Vector2>();
    //
    //     const double siblingDistance = 80.0;
    //     double levelDistance = yNodeSeparator;
    //
    //     var nodes = new Dictionary<RedBlackNode, (double X, double Y)>();
    //     var result = new Dictionary<int, Vector2>();
    //     int inOrderIndex = 0;
    //
    //     CalculatePrelim(root, 0);
    //
    //     float offset = leftPanelWidth / 2f + screenMiddleX - (float)nodes[root].X;
    //     foreach (var kvp in nodes)
    //         result[kvp.Key.Key] = new Vector2((float)kvp.Value.X + offset, (float)kvp.Value.Y + yNodeSeparator);
    //
    //     return result;
    //
    //     void CalculatePrelim(RedBlackNode node, int depth)
    //     {
    //         if (node.IsNil) return;
    //
    //         nodes[node] = (0, depth * levelDistance);
    //
    //         CalculatePrelim(node.Left, depth + 1);
    //
    //         if (node.Left.IsNil && node.Right.IsNil)
    //         {
    //             nodes[node] = (inOrderIndex++ * siblingDistance, nodes[node].Y);
    //             return;
    //         }
    //
    //         CalculatePrelim(node.Right, depth + 1);
    //
    //         double x;
    //         if (node.Left.IsNil)
    //         {
    //             x = nodes[node.Right].X - siblingDistance / 2.0;
    //         }
    //         else if (node.Right.IsNil)
    //         {
    //             x = nodes[node.Left].X + siblingDistance / 2.0;
    //         }
    //         else
    //         {
    //             double leftPos = nodes[node.Left].X;
    //             double rightPos = nodes[node.Right].X;
    //
    //             if (rightPos - leftPos < siblingDistance)
    //             {
    //                 double shift = siblingDistance - (rightPos - leftPos);
    //                 ShiftSubtree(node.Right, shift);
    //                 rightPos += shift;
    //             }
    //
    //             // Check deeper levels for overlap
    //             double minGap = GetContourMinGap(node.Left, node.Right);
    //             if (minGap < siblingDistance)
    //             {
    //                 double shift = siblingDistance - minGap;
    //                 ShiftSubtree(node.Right, shift);
    //                 rightPos += shift;
    //             }
    //
    //             x = (leftPos + rightPos) / 2.0;
    //         }
    //
    //         nodes[node] = (x, nodes[node].Y);
    //     }
    //
    //     void ShiftSubtree(RedBlackNode node, double shift)
    //     {
    //         var stack = new Stack<RedBlackNode>();
    //         stack.Push(node);
    //         while (stack.Count > 0)
    //         {
    //             var current = stack.Pop();
    //             if (current.IsNil) continue;
    //             nodes[current] = (nodes[current].X + shift, nodes[current].Y);
    //             stack.Push(current.Left);
    //             stack.Push(current.Right);
    //         }
    //     }
    //
    //     double GetContourMinGap(RedBlackNode left, RedBlackNode right)
    //     {
    //         double minGap = double.MaxValue;
    //         var leftLevel = new List<RedBlackNode> { left };
    //         var rightLevel = new List<RedBlackNode> { right };
    //
    //         while (leftLevel.Count > 0 && rightLevel.Count > 0)
    //         {
    //             var leftNodes = leftLevel.Where(n => !n.IsNil).ToList();
    //             var rightNodes = rightLevel.Where(n => !n.IsNil).ToList();
    //
    //             if (leftNodes.Count == 0 || rightNodes.Count == 0) break;
    //
    //             double leftMax = leftNodes.Max(n => nodes[n].X);
    //             double rightMin = rightNodes.Min(n => nodes[n].X);
    //             double gap = rightMin - leftMax;
    //             if (gap < minGap) minGap = gap;
    //
    //             leftLevel = leftNodes.SelectMany(n => new[] { n.Left, n.Right }).ToList();
    //             rightLevel = rightNodes.SelectMany(n => new[] { n.Left, n.Right }).ToList();
    //         }
    //
    //         return minGap == double.MaxValue ? siblingDistance : minGap;
    //     }
    // }
}
