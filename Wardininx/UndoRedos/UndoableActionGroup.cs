namespace Wardininx.UndoRedos;
public record class UndoableActionGroup(string ActionName, LinkedList<UndoableAction> UndoableActions) : UndoableAction(
    ActionName,
    delegate
    {
        if (UndoableActions.Count <= 0) return;
        var node = UndoableActions.Last;
        while (node.Previous != UndoableActions.First)
        {
            node.Value.Undo();
            node = node.Previous;
        }
        node.Value.Undo();
    },
    delegate
    {
        if (UndoableActions.Count <= 0) return;
        var node = UndoableActions.First;
        while (node.Next != UndoableActions.Last)
        {
            node.Value.Redo();
            node = node.Next;
        }
        node.Value.Redo();
    },
    wasActionDone =>
    {
        if (UndoableActions.Count <= 0) return;
        var node = UndoableActions.Last;
        while (node.Previous != UndoableActions.First)
        {
            node.Value.Cleanup(wasActionDone);
            node = node.Previous;
        }
        node.Value.Cleanup(wasActionDone);
    }
), IDisposable
{
    public bool IsActive { get; private set; } = true;
    void IDisposable.Dispose()
    {
        IsActive = false;
    }
}