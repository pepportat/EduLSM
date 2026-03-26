using System.Numerics;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Main.Helpers;
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

        var screenWidth = 1200;
        var screenHeight = 800;
        int screenMiddleX;
        int currentStepIndex = 0;
        string input = "";
        

        var treeCamera = new Camera2D
        {
            Zoom = 1.0f
        };

        var leftPanelCamera = new Camera2D
        {
            Zoom = 1.0f
        };


        SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        InitWindow(screenWidth, screenHeight, "Edu LSM");
        var font = LoadFont("./resources/Roboto-Medium.ttf");
        SetTextureFilter(font.Texture, TextureFilter.Bilinear);
        
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
                    treeCamera.Zoom = Clamp((float)Math.Exp(Math.Log(treeCamera.Zoom)+scale), 0.5f, 10.0f);
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
                    
                    DrawTreeArea();
                    
                EndMode2D();

                BeginMode2D(leftPanelCamera);
                    DrawLeftPanel(LeftPanelWidth, 20, 4);
                EndMode2D();
            
            EndDrawing();
        }
        
        UnloadFont(font);
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
        
            if (IsKeyPressed(KeyboardKey.Backspace) && input.Length > 0)
            {
                input = input[..^1];
            }
            
            if (IsKeyDown(KeyboardKey.Enter) && input.Length > 0)
            {
                var (_, list) = manager.Tree.Add(int.Parse(input), $"Data for key {input}");
                steps = list;
                currentStepIndex = 0;
                manager.UpdateLayout(LeftPanelWidth, screenMiddleX);
                input = "";
            }
            
            if (IsKeyDown(KeyboardKey.S) && input.Length > 0)
            {
                var (_, list) = manager.Tree.Get(int.Parse(input));
                steps = list;
                currentStepIndex = 0;
                input = "";
            }
            
            if (IsKeyDown(KeyboardKey.R) && input.Length > 0)
            {
                var (_, list) = manager.Tree.Remove(int.Parse(input));
                steps = list;
                currentStepIndex = 0;
                manager.UpdateLayout(LeftPanelWidth, screenMiddleX);
                input = "";
            }

            if (IsKeyPressed(KeyboardKey.Left))
            {
                if (currentStepIndex > 0)
                {
                    currentStepIndex--;
                }
            }
            
            if (IsKeyPressed(KeyboardKey.Right))
            {
                if (currentStepIndex < steps.Count - 1)
                {
                    currentStepIndex++;
                }
            }
        }
        
        
        void DrawLeftPanel(int width, int fontSize = 16, int separatorHeight = 2)
        {
            DrawRectangle(0, 0, width, screenHeight * 2, Color.Black);
            DrawRectangleLines(0, 0, width, screenHeight * 2, Color.White);
            DrawTextEx(font, $"Count: {manager.Tree.Count}", new(10, 10), fontSize, 2, Color.White);
            DrawTextEx(font, $"Input: {input}", new (10, 10 + separatorHeight + fontSize), fontSize, 2, Color.White);

            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];

                int y = 10 + (i + 2) * separatorHeight + (i + 2) * fontSize;
                
                DrawTextEx(font, step.Description, new(10, y), fontSize, 2, currentStepIndex == i ? Color.White : Color.Gray);
            }


            if (steps.Count == 0)
            {
                return;
            }
            
            var currentStep = steps[currentStepIndex];
            
            if (currentStep.Key.HasValue)
            {
                var layout = steps[currentStepIndex].Layout;

                if (layout is not null)
                {
                    if (layout.TryGetValue(currentStep.Key.Value, out var nodeCord))
                    {
                        BeginMode2D(treeCamera);
                            DrawRing(new Vector2((int)nodeCord.Position.X, (int)nodeCord.Position.Y), 28, 34, 0, 360, 24, GetStepNodeColor(currentStep.Kind));
                        EndMode2D();
                    }
                }
            }
        }
        
        void DrawTreeArea()
        {
            if (steps.Count == 0)
            {
                return;
            }

            var layout = steps[currentStepIndex].Layout;

            if (layout is not null)
            {
                DrawTreeInternal(layout.OffsetLayout(LeftPanelWidth, screenMiddleX));
                return;
            }
            
            DrawTreeInternal(manager.Layout);
        }

        void DrawTreeInternal(Dictionary<int, NodeSnapshot> layout)
        {
            foreach (var kv in layout.Where(node => node.Value.ParentKey != -1))
            {
                DrawEdges(kv.Value, layout);
            }

            foreach (var kv in layout)
            {
                DrawNode(kv.Value, fontSize: 20);
            }
                    
            foreach (var kv in layout)
            {
                AddOnHoverForNodes(kv.Value, fontSize: 20);
            }
        }
        
        void DrawNode(NodeSnapshot node, int radius = 20, int fontSize = 10)
        {
            var nodeColor = node.Color == NodeColor.Black ? new Color( 30, 41, 59,255) : new Color(220, 38, 38,255);

            int circleX = (int)node.Position.X;
            int circleY = (int)node.Position.Y;
        
            DrawCircle(circleX, circleY, radius, nodeColor);
        
            int textWidth = MeasureText($"{node.Key}", fontSize);
        
            DrawTextEx(font, $"{node.Key}", new(circleX - textWidth / 2f, circleY - fontSize / 2f), fontSize, 2, Color.White);

            if (node.IsTombstone)
            {
                DrawTextEx(font, "t", new (circleX + 8, circleY - 20), 15, 2, Color.White);
            }
        }
        
        void DrawEdges(NodeSnapshot node, Dictionary<int, NodeSnapshot> layout)
        {
            var start = node.Position;
            var end = layout[node.ParentKey].Position;

            DrawLineEx(
                start,
                end,
                1.5f,
                Color.White
            );
        }
        
        void AddOnHoverForNodes(NodeSnapshot node, int radius = 20, int fontSize = 10)
        {
            int circleX = (int)node.Position.X;
            int circleY = (int)node.Position.Y;
                
            var mousePos = GetMousePosition();                                                                                                                                                                                                
            var mouseWorldPos = GetScreenToWorld2D(mousePos, treeCamera);  // Convert mouse to world coords                                                                                                                                       
            if (CheckCollisionPointRec(mouseWorldPos, new Rectangle(circleX - radius, circleY - radius, radius * 2, radius * 2)))                                                                                                             
            {                                                                                                                                                                                                                                 
                DrawCircleLines(circleX, circleY, radius, Color.White);           
                DrawTextEx(font, $"{node.Value}", new(circleX + 15, circleY - 30), fontSize, 2, Color.White);                                                                                                                                                
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
}
