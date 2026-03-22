using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Raylib_cs;

namespace Main.Helpers;

public static class StepColorHelper
{
    public static Color GetStepNodeColor(StepKind stepKind)
    {
        return stepKind switch {
            StepKind.SearchStart => Color.White,
            StepKind.SearchCompare => Color.Blue,
            StepKind.SearchHit => Color.Green,
            StepKind.SearchMiss => Color.Green,
            StepKind.InsertStart => Color.DarkGray,
            StepKind.InsertTraverse => Color.Brown,
            StepKind.InsertAttach => Color.DarkBrown,
            StepKind.DeleteTombstone => Color.Pink,
            
            _ => Color.Gold //TODO: add other colors
            // StepKind.InsertDuplicateUpdate => expr,
            // StepKind.InsertColour => expr,
            // StepKind.DeleteStart => expr,
            // 
            // StepKind.DeleteAlreadyTombstoned => expr,
            // StepKind.DeleteNotFound => expr,
            // StepKind.FixupStart => expr,
            // StepKind.FixupCase => expr,
            // StepKind.Recolour => expr,
            // StepKind.Rotation => expr,
            // StepKind.MoveUp => expr,
            // StepKind.FixupDone => expr,
        };
    }
}