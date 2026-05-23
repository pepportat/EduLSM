using System.Numerics;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Core.SSTables;
using Main.Helpers;
using Raylib_cs;
using static Main.Helpers.CustomGridHelper;
using static Main.Helpers.StepColorHelper;
using static Main.Helpers.CameraHelpers;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;

namespace Main.UIHandlers;

public partial class LsmEngine
{
    private Camera2D _treeCamera = new()
    {
        Zoom = 1.0f
    };

    private Camera2D _leftPanelCamera = new()
    {
        Zoom = 1.0f
    };
    
    private readonly int _maxMemTableCount;
    
    public void DrawMemTable()
    {
        if (IsMouseButtonDown(MouseButton.Left) && !CheckCollisionPointRec(GetMousePosition(), new Rectangle(0, 0, UIState.LeftPanelWidth, UiState.ScreenHeight)))
        {
           HandleCameraPan(ref _treeCamera);
        }

        float wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            if (CheckCollisionPointRec(GetMousePosition(), new Rectangle(0, 0, UIState.LeftPanelWidth, UiState.ScreenHeight)))
            {
                _leftPanelCamera.Offset.Y += wheel * 20;
                _leftPanelCamera.Offset.Y = Clamp(_leftPanelCamera.Offset.Y, -UiState.ScreenHeight, 0);
            }
            else
            {
                HandleCameraZoom(ref _treeCamera, wheel);
            }
        }

        HandleMemTableInput();

        BeginMode2D(_treeCamera);
            Rlgl.PushMatrix();
                Rlgl.Translatef(1000, 2600, 0);
                Rlgl.Rotatef(90, 1, 0, 0);
                DrawGridCustom(200, 50, Color.DarkGray);
            Rlgl.PopMatrix();

            DrawTreeArea();
        EndMode2D();

        BeginMode2D(_leftPanelCamera);
            DrawLeftPanel(UIState.LeftPanelWidth, 20, 4);
        EndMode2D();

        DrawFlushButton();

        if (TryGetCurrentStep(out var currentStep))
        {
            DrawTextEx(Font, currentStep.Description, new Vector2(UIState.LeftPanelWidth + 10, 10), 20, 2, Color.White);
        }

        DrawTextEx(Font,
            "LB - Pan | MW - Zoom | Enter - Insert | R - Remove | S - Search | </> - step selection",
            new Vector2(UIState.LeftPanelWidth + 10, UiState.ScreenHeight - 28), 18, 2, Color.Gray);
    }

    private void HandleMemTableInput()
    {
        int key = GetKeyPressed();
        char c = (char)key;
        
        // Check if more characters can be added
        if (c is >= '0' and <= '9' && (UiState.Input.Length < UIState.MaxInputChars))
        {
            UiState.Input += (char)key;
        }
        
        if (IsKeyPressed(KeyboardKey.Backspace) && UiState.Input.Length > 0)
        {
            UiState.Input = UiState.Input[..^1];
        }
            
        if (IsKeyDown(KeyboardKey.S) && UiState.Input.Length > 0)
        {
            Search();
            UiState.CurrentStepIndex = 0;
            UiState.Input = "";
        }
            
        if (IsKeyDown(KeyboardKey.R) && UiState.Input.Length > 0)
        {
            if (Tree.Count >= _maxMemTableCount)
            {
                UiState.Input = "";
                return;
            }
            
            var (_, list) = Tree.Remove(int.Parse(UiState.Input));
            Steps = list;
            UiState.CurrentStepIndex = 0;
            UpdateLayout();
            UiState.Input = "";
        }

        if (IsKeyPressed(KeyboardKey.Left))
        {
            if (UiState.CurrentStepIndex > 0)
            {
                UiState.CurrentStepIndex--;
            }
        }
            
        if (IsKeyPressed(KeyboardKey.Right))
        {
            if (UiState.CurrentStepIndex < Steps.Count - 1)
            {
                UiState.CurrentStepIndex++;
            }
        }
        
        if (IsKeyDown(KeyboardKey.Enter) && UiState.Input.Length > 0)
        {
            if (Tree.Count >= _maxMemTableCount)
            {
                UiState.Input = "";
                return;
            }

            var value = Faker.Random.Word();
            
            var (_, list) = Tree.Add(int.Parse(UiState.Input), value[..Math.Min(value.Length, 16)]);
            Steps = list;
            UiState.CurrentStepIndex = 0;
            UpdateLayout();
            UiState.Input = "";
        }
    }

    private void DrawLeftPanel(int width, int fontSize = 16, int separatorHeight = 2)
    {
        BeginScissorMode(0, 0, width, UiState.ScreenHeight * 2);

        DrawRectangle(0, 0, width, UiState.ScreenHeight * 2, Color.Black);
        DrawRectangleLines(0, 0, width, UiState.ScreenHeight * 2, Color.White);
        DrawTextEx(Font, $"Count: {Tree.Count} - Max Count: {_maxMemTableCount}", new Vector2(10, 10), fontSize, 2, Color.White);
        DrawTextEx(Font, $"Input: {UiState.Input}", new Vector2(10, 10 + separatorHeight + fontSize), fontSize, 2, Color.White);

        for (var i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];

            int y = 10 + (i + 2) * separatorHeight + (i + 2) * fontSize;

            DrawTextEx(Font, step.Description, new Vector2(10, y), fontSize, 2,
                UiState.CurrentStepIndex == i ? Color.White : Color.Gray);
        }

        EndScissorMode();
        
        
        if (TryGetCurrentStep(out var currentStep))
        {
            if (currentStep is { Key: not null, Layout: not null })
            {
                var layout = currentStep.Layout!.OffsetLayout(UIState.LeftPanelWidth, UiState.ScreenMiddleX);
                
                if (layout.TryGetValue(currentStep.Key.Value, out var nodeCord))
                {
                    BeginMode2D(_treeCamera);
                    DrawRing(new Vector2((int)nodeCord.Position.X, (int)nodeCord.Position.Y), 28, 34, 0, 360, 24,
                        GetStepNodeColor(currentStep.Kind));
                    EndMode2D();
                }
            }
        }
    }

    private void DrawTreeArea()
    {
        if (TryGetCurrentStep(out var currentStep))
        {
            var layout = currentStep.Layout;

            if (layout is not null)
            {
                DrawTreeInternal(layout.OffsetLayout(UIState.LeftPanelWidth, UiState.ScreenMiddleX));
                return;
            }
        }

        DrawTreeInternal(Layout);
    }

    private void DrawTreeInternal(Dictionary<int, NodeSnapshot> layout)
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

    private void DrawNode(NodeSnapshot node, int radius = 20, int fontSize = 10)
    {
        var nodeColor = node.Color == NodeColor.Black ? new Color(30, 41, 59, 255) : new Color(220, 38, 38, 255);

        int circleX = (int)node.Position.X;
        int circleY = (int)node.Position.Y;

        DrawCircle(circleX, circleY, radius, nodeColor);

        var textWidth = MeasureTextEx(Font, $"{node.Key}", fontSize, 2).X;

        DrawTextEx(Font, $"{node.Key}", new(circleX - textWidth / 2f, circleY - fontSize / 2f), fontSize, 2,
            Color.White);

        if (node.IsTombstone)
        {
            DrawTextEx(Font, "t", new(circleX + 8, circleY - 20), 15, 2, Color.White);
        }
    }

    private void DrawEdges(NodeSnapshot node, Dictionary<int, NodeSnapshot> layout)
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

    private void DrawFlushButton()
    {
        int buttonWidth = 100;
        int buttonHeight = 40;

        int buttonX = UiState.ScreenWidth - buttonWidth - 10;
        int buttonY = 10;
        
        var rect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

        bool readyToFlush = Tree.Count == _maxMemTableCount;
        
        
        DrawRectangleRounded(rect, 0.4f, 10, readyToFlush ? Color.Red : Color.Gray);
        
        var textWidth = MeasureTextEx(Font, "Flush", 20, 2).X;
        
        var textCord = new Vector2(buttonX + textWidth / 2f, buttonY + 10);
        DrawTextEx(Font, "Flush", textCord,  20, 2, readyToFlush ? Color.White : Color.LightGray);

        if (readyToFlush && CheckCollisionPointRec(GetMousePosition(), rect))
        {
            DrawRectangleRoundedLines(rect, 0.4f, 10, Color.White);

            if (IsMouseButtonPressed(MouseButton.Left))
            {
                var ssTable = Flush.FlushMemTable(Tree.GetSorted(), _dataPath);
                SsTables = [ssTable, ..SsTables];
                Tree.Clear();
                Layout = Tree.GetLayout();
                Steps = [];
            }
        }
    }
    
    private void AddOnHoverForNodes(NodeSnapshot node, int radius = 20, int fontSize = 10)
    {
        int circleX = (int)node.Position.X;
        int circleY = (int)node.Position.Y;

        var mousePos = GetMousePosition();
        var mouseWorldPos = GetScreenToWorld2D(mousePos, _treeCamera); // Convert mouse to world coords                                                                                                                                       
        if (CheckCollisionPointRec(mouseWorldPos, new Rectangle(circleX - radius, circleY - radius, radius * 2, radius * 2)))
        {
            DrawCircleLines(circleX, circleY, radius, Color.White);
            DrawTextEx(Font, $"{node.Value}", new(circleX + 15, circleY - 30), fontSize, 2, Color.White);
        }
    }
}