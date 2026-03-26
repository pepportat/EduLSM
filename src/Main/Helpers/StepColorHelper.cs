using Core.MemTables.RedBlackTree.VisualizerHelpers;
using Raylib_cs;

namespace Main.Helpers;

public static class StepColorHelper
{
    public static Color GetStepNodeColor(StepKind stepKind)
    {
        return stepKind switch
        {
            StepKind.SearchStart    => new(20,  184, 166, 255),   // teal-500
            StepKind.SearchCompare  => new(94,  234, 212, 255),   // teal-300
            StepKind.SearchHit      => new(52,  211, 153, 255),   // emerald-400
            StepKind.SearchMiss     => new(148, 163, 184, 255),   // slate-400
            
            StepKind.InsertStart            => new(245, 158, 11,  255),  // amber-500
            StepKind.InsertTraverse         => new(253, 211, 77,  255),  // amber-300
            StepKind.InsertAttach           => new(251, 191, 36,  255),  // amber-400
            StepKind.InsertDuplicateUpdate  => new(249, 115, 22,  255),  // orange-500
            
            StepKind.DeleteStart            => new(244, 114, 182, 255),  // pink-400
            StepKind.DeleteTombstone        => new(190, 18,  60,  255),  // rose-800
            StepKind.DeleteAlreadyTombstoned=> new(100, 116, 139, 255),  // slate-500
            StepKind.DeleteNotFound         => new(100, 116, 139, 255),  // slate-500
            
            StepKind.FixupCase  => new(129, 140, 248, 255),   // indigo-400

            StepKind.Rotation   => new(34,  211, 238, 255),   // cyan-400
            StepKind.Recolour   => new(192, 132, 252, 255),   // purple-400

            _ => new(148, 163, 184, 255),
        };
    }
}