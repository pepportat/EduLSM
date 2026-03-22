namespace Core.MemTables.RedBlackTree.VisualizerHelpers;

public enum StepKind
{
    SearchStart,
    SearchCompare,
    SearchHit,
    SearchMiss,
    
    InsertStart,
    InsertTraverse,
    InsertAttach,
    InsertDuplicateUpdate,
    InsertColour,
    
    DeleteStart,
    DeleteTombstone,
    DeleteAlreadyTombstoned,
    DeleteNotFound,
    
    FixupStart,
    FixupCase,
    Recolour,
    Rotation,
    MoveUp,
    FixupDone
}