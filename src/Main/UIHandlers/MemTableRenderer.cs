using System.Numerics;
using Bogus;
using Core.Compaction;
using Core.MemTables.RedBlackTree;
using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Core.SSTables;
using Main.Components;
using Main.Helpers;
using Raylib_cs;
using static Main.Helpers.CustomGridHelper;
using static Main.Helpers.StepColorHelper;
using static Main.Helpers.CameraHelpers;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;
using static Core.SSTables.Search;

namespace Main.UIHandlers;

public class MemTableRenderer
{
    private Camera2D _treeCamera = new()
    {
        Zoom = 1.0f
    };

    private Camera2D _leftPanelCamera = new()
    {
        Zoom = 1.0f
    };

    private RedBlackTree Tree { get; set; }
    private Dictionary<int, NodeSnapshot> Layout { get; set; }
    private List<MemTableStep> Steps { get; set; }
    private Faker Faker { get; set; }
    private readonly UIState _uiState;
    private readonly int _maxMemTableCount;
    private readonly string _dataPath;
    private readonly SsTableRenderer _ssTableRenderer;
    public Font Font { get; set; }

    public MemTableRenderer(string dataPath, int maxMemTableCount, UIState uiState, SsTableRenderer ssTableRenderer)
    {
        Tree = new RedBlackTree();
        Layout = Tree.GetLayout();
        Steps = [];
        _dataPath = dataPath;
        _maxMemTableCount = maxMemTableCount;
        _uiState = uiState;
        _ssTableRenderer = ssTableRenderer;
        Faker = new Faker();
    }

    private void Search()
    {
        var key = int.Parse(_uiState.Input);
        var (_, list) = Tree.Get(key);
        Steps = list;
        _ssTableRenderer.SsTablesSearchResults = SearchKey(key, _dataPath).ToList();
    }

    private void UpdateLayout()
    {
        Layout = Tree.GetLayout();
    }

    private bool InputEnabled()
    {
        if (Tree.Count >= _maxMemTableCount)
        {
            return false;
        }

        if (_ssTableRenderer.SsTableTabState == SsTableTabState.Compacting)
        {
            return false;
        }

        return true;
    }

    private bool TryGetCurrentStep(out MemTableStep step)
    {
        if (Steps.Count != 0)
        {
            step = Steps[_uiState.CurrentStepIndex];
            return true;
        }

        step = null!;
        return false;
    }

    public void DrawMemTable()
    {
        if (IsMouseButtonDown(MouseButton.Left) && !CheckCollisionPointRec(GetMousePosition(), new Rectangle(0, 0, UIState.LeftPanelWidth, _uiState.ScreenHeight)))
        {
           HandleCameraPan(ref _treeCamera);
        }

        float wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            if (CheckCollisionPointRec(GetMousePosition(), new Rectangle(0, 0, UIState.LeftPanelWidth, _uiState.ScreenHeight)))
            {
                _leftPanelCamera.Offset.Y += wheel * 20;
                _leftPanelCamera.Offset.Y = Clamp(_leftPanelCamera.Offset.Y, -_uiState.ScreenHeight, 0);
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
            "LB - Pan | MW - Zoom | Enter - Insert | R - Remove | S - Search | Up/Down - Step selection",
            new Vector2(UIState.LeftPanelWidth + 10, _uiState.ScreenHeight - 28), 18, 2, Color.Gray);
    }

    private void HandleMemTableInput()
    {
        int key = GetKeyPressed();
        char c = (char)key;

        // Check if more characters can be added
        if (c is >= '0' and <= '9' && (_uiState.Input.Length < UIState.MaxInputChars))
        {
            _uiState.Input += c;
        }

        if (IsKeyPressed(KeyboardKey.Backspace) && _uiState.Input.Length > 0)
        {
            _uiState.Input = _uiState.Input[..^1];
        }

        if (IsKeyPressed(KeyboardKey.S) && _uiState.Input.Length > 0)
        {
            Search();
            _uiState.CurrentStepIndex = 0;
            _uiState.Input = "";
        }

        if (IsKeyPressed(KeyboardKey.R) && _uiState.Input.Length > 0)
        {
            if (Tree.Count >= _maxMemTableCount)
            {
                _uiState.Input = "";
                return;
            }

            var (_, list) = Tree.Remove(int.Parse(_uiState.Input));
            Steps = list;
            _uiState.CurrentStepIndex = 0;
            UpdateLayout();
            _uiState.Input = "";
        }

        if (IsKeyPressed(KeyboardKey.Up))
        {
            if (_uiState.CurrentStepIndex > 0)
            {
                _uiState.CurrentStepIndex--;
            }
        }

        if (IsKeyPressed(KeyboardKey.Down))
        {
            if (_uiState.CurrentStepIndex < Steps.Count - 1)
            {
                _uiState.CurrentStepIndex++;
            }
        }

        if (IsKeyPressed(KeyboardKey.Enter) && _uiState.Input.Length > 0)
        {
            if (!InputEnabled())
            {
                _uiState.Input = "";
                return;
            }

            var value = Faker.Random.Word();

            var (_, list) = Tree.Add(int.Parse(_uiState.Input), value[..Math.Min(value.Length, 16)]);
            Steps = list;
            _uiState.CurrentStepIndex = 0;
            UpdateLayout();
            _uiState.Input = "";
        }
    }

    private void DrawLeftPanel(int width, int fontSize = 16, int separatorHeight = 2)
    {
        BeginScissorMode(0, 0, width, _uiState.ScreenHeight * 2);

            DrawRectangle(0, 0, width, _uiState.ScreenHeight * 2, Color.Black);
            DrawRectangleLines(0, 0, width, _uiState.ScreenHeight * 2, Color.White);
            DrawTextEx(Font, $"Count: {Tree.Count} - Max Count: {_maxMemTableCount}", new Vector2(10, 10), fontSize, 2, Color.White);
            DrawTextEx(Font, $"Input: {_uiState.Input}", new Vector2(10, 10 + separatorHeight + fontSize), fontSize, 2, Color.White);

            for (var i = 0; i < Steps.Count; i++)
            {
                var step = Steps[i];

                int y = 10 + (i + 2) * (separatorHeight + fontSize);

                DrawTextEx(Font, step.Description, new Vector2(10, y), fontSize, 2,
                    _uiState.CurrentStepIndex == i ? Color.White : Color.Gray);
            }

        EndScissorMode();


        if (TryGetCurrentStep(out var currentStep))
        {
            if (currentStep is { Key: not null })
            {
                var layout = currentStep.Layout.OffsetLayout(UIState.LeftPanelWidth, _uiState.ScreenMiddleX);

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

            DrawTreeInternal(layout.OffsetLayout(UIState.LeftPanelWidth, _uiState.ScreenMiddleX));
            return;
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
        var mouseCursor = Button.DrawActionButton(
            _uiState.ScreenWidth - 100 - 10,
            "Flush",
            0,
            Tree.Count == _maxMemTableCount && _ssTableRenderer.CompactionNeededTiers.Count == 0,
            Font,
            () =>
            {
                var ssTable = Flush.FlushMemTable(Tree.GetSorted(), _dataPath);

                if (_ssTableRenderer.SsTables.TryGetValue(1, out var tier)) {
                    _ssTableRenderer.SsTables[1] = tier.Prepend(ssTable).ToList();
                } else {
                    _ssTableRenderer.SsTables[1] = [ssTable];
                }

                _ssTableRenderer.CompactionNeededTiers = TierMonitor.NeedsCompaction(_dataPath);

                Tree.Clear();
                Layout = Tree.GetLayout();
                Steps = [];
            }
        );

        _uiState.SetCurrentMouseCursor(mouseCursor);
    }

    private void AddOnHoverForNodes(NodeSnapshot node, int radius = 20, int fontSize = 10)
    {
        int circleX = (int)node.Position.X;
        int circleY = (int)node.Position.Y;

        var mousePos = GetMousePosition();
        var mouseWorldPos = GetScreenToWorld2D(mousePos, _treeCamera);
        if (CheckCollisionPointRec(mouseWorldPos, new Rectangle(circleX - radius, circleY - radius, radius * 2, radius * 2)))
        {
            DrawCircleLines(circleX, circleY, radius, Color.White);
            DrawTextEx(Font, $"{node.Value}", new(circleX + 15, circleY - 30), fontSize, 2, Color.White);
        }
    }
}