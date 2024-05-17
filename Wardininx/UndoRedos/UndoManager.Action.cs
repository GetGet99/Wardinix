namespace Wardininx.UndoRedos;

partial class UndoManager
{
    public void AddAction(UndoableAction action)
    {
        if (Redos.Count > 0) ClearRedos();
        while (Groups.TryPeek(out var group))
        {
            if (!group.IsActive)
            {
                // this is not supposed to be here anymore
                Groups.Pop();
                // retry
                continue;
            }
            else
            {
                // add to group
                group.UndoableActions.AddLast(action);
                return;
            }
        }
        // fallback
        Undos.AddLast(action);
        _IsUndoable.Value = true;
    }
    public void DoAndAddAction(UndoableAction action)
    {
        action.Redo();
        AddAction(action);
    }
}
