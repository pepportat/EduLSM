using System.Numerics;
using Core.Compaction;
using Core.SSTables.Structure;
using Core.SSTables.VisualizerHelpers;
using Main.Components;
using Main.Helpers;
using Raylib_cs;
using static Main.Helpers.CameraHelpers;
using static Main.Helpers.CustomGridHelper;
using static Raylib_cs.Raylib;
using static Main.Components.CardSegment;
using static Core.Common.FileNameHelpers;

namespace Main.UIHandlers;

public partial class LsmEngine
{
    private const int CardWidth = 400;
    private const int CardSpacing = CardWidth + 60;
    private const int StartX = 40;
    private const int StartY = 40;
    private const int FontSize = 16;
    private const int RowHeight = 22;
    private const int SectionPadding = 10;
    private const int BitCellSize = 10;
    private const int SparseSampleInterval = 10;
    
    private Camera2D _ssTableCamera = new()
    {
        Zoom = 1.0f
    };

    private Dictionary<int, List<SsTable>> SsTables { get; set; }
    private SsTableTabState _ssTableTabState;
    private List<int> CompactionNeededTiers { get; set; }
    private int? CleanupNeededTier { get; set; }
    private CompactionVisualizerData? _compactionVisualizerData;
    
    public void DrawSsTableScreen()
    {
        var wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            HandleCameraZoom(ref _ssTableCamera, wheel);
        }
        
        if (IsMouseButtonDown(MouseButton.Left))
        {
            HandleCameraPan(ref _ssTableCamera);
        }
        
        BeginMode2D(_ssTableCamera);
            Rlgl.PushMatrix();
                Rlgl.Translatef(-500, 4500, 0);
                Rlgl.Rotatef(90, 0, 0, 1);
                Rlgl.Rotatef(90, 1, 0, 0);
                DrawGridCustom(200, 50, Color.DarkGray);
            Rlgl.PopMatrix();
            
            var runningY = StartY;
            
            foreach (var tier in SsTables.OrderBy(t => t.Key))
            {
                runningY += DrawSsTableTier(StartX, runningY, tier.Value, tier.Key);
            }

        EndMode2D();
        
        DrawCompactButton();
        DrawFileCleanupButton();
    }

    private int DrawSsTableTier(int x, int y, List<SsTable> ssTables, int tier)
    {
        List<int> ssTableHeights = [];
        
        DrawTextEx(Font, $"Tier {tier} (newest to oldest)", new Vector2(x, y), 28, 2, Color.White);
        
        var allSsTableWidth = ssTables.Count * CardSpacing;
        DrawLine(x, y + 34, x + allSsTableWidth, y + 34, Color.White);
        
        for (var i = 0; i < ssTables.Count; i++)
        {
            int ssTableX = x + i * CardSpacing;
            
            var ssTable = ssTables[i];
            ssTableHeights.Add(DrawSsTable(ssTable, ssTableX, y + 44));
        }
        
        return ssTableHeights.DefaultIfEmpty(0).Max() + 72;
    }
    
    private int DrawSsTable(SsTable ssTable, int x, int y)
    {
        var startingY = y;
        
        var dataBlock = ssTable.KvpList.ToList();
        var sparseIndex = ssTable.Index;
        var bloom = ssTable.BloomFilter;
        var meta = ssTable.Footer;
        
        var searchResult = SsTablesSearchResults.SingleOrDefault(s => s.FileName == ssTable.FileName);
        
        List<(int arrowY, int key)> arrowSources = [];

        // Header
        y += DrawHeader() + 8;

        // Bloom Filter
        y += DrawBloomFilter() + 8;

        // Sparse Index
        y += DrawSparseIndex() + 8;

        // Data Block
        var dataBlockStartY = DrawDataBlock(out int datablockHeight);
        y += datablockHeight;

        // Sparse Index arrows
        DrawSparseIndexConnections(dataBlockStartY);

        return y - startingY;

        int DrawHeader()
        {
            const int headerHeight = RowHeight + SectionPadding;

            var color = new Color(30, 41, 59, 255);

            if (_compactionVisualizerData is not null && _compactionVisualizerData.CompactedTables.Contains(ssTable))
            {
                color = _compactionVisualizerData.SsTableColor(ssTable);
            }
            
            DrawRectangleRounded(new Rectangle(x, y, CardWidth, headerHeight), 0.3f, 8, color);
            DrawTextEx(Font, $"Records: {meta.TotalRecordCount}  Sparse Index Count: {meta.BlockCount}",
                new Vector2(x + SectionPadding, y + SectionPadding), FontSize, 2, Color.White);

            return headerHeight;
        }

        int DrawBloomFilter()
        {
            var bitCols = Math.Min(bloom.Size, (CardWidth - SectionPadding * 2) / (BitCellSize + 2));
            var bloomRows = (int)Math.Ceiling((double)bloom.Size / bitCols);
            var bloomHeight = bloomRows * (BitCellSize + 2) + RowHeight + SectionPadding * 2;


            bool? foundInBloomFilter = searchResult?.FoundInBloomFilter;
            
            Color bloomColor = foundInBloomFilter switch
            {
                true => Color.Green,
                false => Color.Red,
                null => Color.DarkGray
            };
            
            DrawCardSegment(x, y, CardWidth, bloomHeight, new Color(15, 23, 42, 255), bloomColor);
            DrawTextEx(Font, $"Bloom Filter - Bits:{bloom.Size} - Found({searchResult?.FoundInBloomFilter.ToString() ?? "N/A"})", new Vector2(x + SectionPadding, y + SectionPadding), FontSize, 2,
                Color.Gray);

            int gridWidth = bitCols * (BitCellSize + 2) - 2;
            int bitStartX = x + (CardWidth - gridWidth) / 2;
            int bitStartY = y + SectionPadding + RowHeight;

            for (int b = 0; b < bloom.Size; b++)
            {
                int col = b % bitCols;
                int row = b / bitCols;
                int bx = bitStartX + col * (BitCellSize + 2);
                int by = bitStartY + row * (BitCellSize + 2);
                DrawRectangle(bx, by, BitCellSize, BitCellSize,
                    bloom.Bits[b] ? new Color(99, 102, 241, 255) : new Color(30, 41, 59, 255));
                DrawRectangleLines(bx, by, BitCellSize, BitCellSize, Color.DarkGray);
            }

            return bloomHeight;
        }

        int DrawSparseIndex()
        {
            var sparseHeight = sparseIndex.KeyOffsetPairs.Count * RowHeight + RowHeight + SectionPadding * 2;

            Color sparseIndexBorderColor = searchResult?.FoundInSparseIndex switch
            {
                true => Color.Green,
                _ => Color.DarkGray
            };
            
            DrawCardSegment(x, y, CardWidth, sparseHeight, new Color(15, 23, 42, 255), sparseIndexBorderColor);
            DrawTextEx(Font, "Sparse Index", new Vector2(x + SectionPadding, y + SectionPadding), FontSize, 2,
                Color.Gray);

            int sparseRowStartY = y + SectionPadding + RowHeight;

            for (int s = 0; s < sparseIndex.KeyOffsetPairs.Count; s++)
            {
                bool? usedSparseIndexEntry = searchResult?.SparseIndexKey == sparseIndex.KeyOffsetPairs[s].Key;
                
                Color sparseIndexColor = usedSparseIndexEntry switch
                {
                    true => Color.Green,
                    _ => new Color(50, 60, 80, 255)
                };
                
                var entry = sparseIndex.KeyOffsetPairs[s];
                int ry = sparseRowStartY + s * (RowHeight + 1);
                DrawRectangleLines(x + SectionPadding, ry, CardWidth - SectionPadding * 2, RowHeight,
                    sparseIndexColor);
                DrawTextEx(Font, $"Key: {entry.Key}  ->  offset: {entry.Offset}",
                    new Vector2(x + SectionPadding + 4, ry + 3), FontSize, 2, Color.White);
                arrowSources.Add((ry + RowHeight / 2, entry.Key));
            }

            return sparseHeight;
        }

        int DrawDataBlock(out int dataHeight)
        {
            dataHeight = dataBlock.Count * (RowHeight + 1) + RowHeight + SectionPadding * 2;
            
            DrawCardSegment(x, y, CardWidth, dataHeight, new Color(15, 23, 42, 255), Color.DarkGray);
            DrawTextEx(Font, "Data Block", new Vector2(x + SectionPadding, y + SectionPadding), FontSize, 2,
                Color.Gray);

            int dataRowStartY = y + SectionPadding + RowHeight;

            for (int d = 0; d < dataBlock.Count; d++)
            {
                var kvp = dataBlock[d];
                int ry = dataRowStartY + d * (RowHeight + 1);

                if (kvp.IsTombStoned)
                    DrawRectangle(x + SectionPadding, ry, CardWidth - SectionPadding * 2, RowHeight,
                        new Color(220, 38, 38, 60));

                if (_compactionVisualizerData is not null && _compactionVisualizerData.CompactionResult == ssTable)
                {
                    DrawRectangle(x + SectionPadding, ry, CardWidth - SectionPadding * 2, RowHeight,
                        _compactionVisualizerData.KeyColor(kvp.Key));
                }
                
                Color keyColor = searchResult?.KeyValuePair?.Key == kvp.Key ? Color.Green : new Color(50, 60, 80, 255);
                
                DrawRectangleLines(x + SectionPadding, ry, CardWidth - SectionPadding * 2, RowHeight,
                    keyColor);
                DrawTextEx(Font, $"{kvp.Key}", new Vector2(x + SectionPadding + 4, ry + 3), FontSize, 2, Color.White);
                
                DrawTextEx(Font, kvp.Value, new Vector2(x + SectionPadding + 80, ry + 3), FontSize, 2, Color.White);
            }

            return dataRowStartY;
        }

        void DrawSparseIndexConnections(int dataStartY)
        {
            for (var i = 0; i < arrowSources.Count; i++)
            {
                var (arrowY, key) = arrowSources[i];
                int dataRowIndex = dataBlock.FindIndex(k => k.Key == key);

                int targetY = dataStartY + dataRowIndex * (RowHeight + 1) + RowHeight / 2;
                int arrowX = x + CardWidth - SectionPadding;

                var sparseIndexUsed = searchResult?.SparseIndexKey == key;
                
                var lineColor = sparseIndexUsed ? Color.Green : Color.White;

                DrawLineEx(
                    new Vector2(arrowX, arrowY),
                    new Vector2(arrowX + (i + 1) * 16, arrowY),
                    1.5f,
                    lineColor
                );

                DrawLineEx(
                    new Vector2(arrowX + (i + 1) * 16, arrowY),
                    new Vector2(arrowX + ((i + 1) * 16), targetY),
                    1.5f,
                    lineColor
                );

                DrawLineEx(
                    new Vector2(arrowX + ((i + 1) * 16), targetY),
                    new Vector2(arrowX, targetY),
                    1.5f,
                    lineColor
                );

                if (sparseIndexUsed)
                {
                    var lineX = x + SectionPadding / 2f;
                    var lineYStart = targetY - (RowHeight + 1) / 2f;
                    var lineYEnd = targetY + (SparseSampleInterval - 0.5f) * (RowHeight + 1);

                    if (dataBlock.Count - SparseSampleInterval < dataRowIndex)
                    {
                        var numRowsToInclude = dataBlock.Count - dataRowIndex - 0.5f;
                        lineYEnd = targetY + numRowsToInclude * (RowHeight + 1);
                    }
                    
                    DrawLineEx(startPos: new Vector2(lineX, lineYStart), endPos: new Vector2(lineX, lineYEnd), SectionPadding, Color.Green);
                }
            }
        }
    }

    private void DrawCompactButton()
    {
        Button.DrawActionButton(
            UiState.ScreenWidth - 100 - 10,
            "Compact",
            0,
            CompactionNeededTiers.Count != 0 && CleanupNeededTier is null,
            Font,
            () =>
            {
                _ssTableTabState = SsTableTabState.Compacting;
                var (ssTable, keySources) = SsTableCompacter.CompactFiles(_dataPath, CompactionNeededTiers[0]);

                var tierAddedTo = GetSsTableFileTier(ssTable.FileName);

                if (SsTables.TryGetValue(tierAddedTo, out var tier))
                {
                    SsTables[tierAddedTo] = tier.Prepend(ssTable).ToList();
                }
                else
                {
                    SsTables[tierAddedTo] = [ssTable];
                }

                _compactionVisualizerData = new CompactionVisualizerData
                (
                    compactedTables: SsTables[CompactionNeededTiers[0]],
                    compactionResult: ssTable,
                    keySources: keySources
                );

                CleanupNeededTier = CompactionNeededTiers[0];
                CompactionNeededTiers.RemoveAt(0);
            }
        );
    }

    private void DrawFileCleanupButton()
    {
        Button.DrawActionButton(
            UiState.ScreenWidth - 100 - 10,
            "Cleanup",
            1,
            CleanupNeededTier is not null, 
            Font,
            () =>
            {
                CleanupNeededTier = null;
                _compactionVisualizerData = null;
                SsTables = ReadAllSsTables.ReadAllTables(_dataPath);

                if (CompactionNeededTiers.Count == 0)
                {
                    _ssTableTabState = SsTableTabState.Viewing;
                }
            }
        );
    }
}