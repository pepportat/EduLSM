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
    
    DeleteStart,
    DeleteTombstone,
    DeleteAlreadyTombstoned,
    DeleteNotFound,
    
    FixupCase,
    Recolour,
    Rotation,
}