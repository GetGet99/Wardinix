namespace Wardininx.UndoRedos;
partial class UndoManager
{
    public UndoManager()
    {
        IsUndoableProperty = new ReadOnlyProperty<bool>(_IsUndoable);
        IsRedoableProperty = new ReadOnlyProperty<bool>(_IsRedoable);
    }
    LinkedList<UndoableAction> Undos = new(), Redos = new();
    readonly Property<bool> _IsUndoable = new(false), _IsRedoable = new(false);
    public partial void Undo()
    {
        if (!_IsUndoable.Value) throw new InvalidOperationException();
        var last = Undos.Last.Value;
        Undos.RemoveLast();
        Redos.AddFirst(last);
        last.Undo();
        _IsUndoable.Value = Undos.Count is not 0;
        _IsRedoable.Value = true;
    }
    public partial void Redo()
    {
        if (!_IsRedoable.Value) throw new InvalidOperationException();
        var first = Redos.First.Value;
        Undos.AddLast(first);
        Redos.RemoveFirst();
        first.Redo();
        _IsUndoable.Value = true;
        _IsRedoable.Value = Redos.Count is not 0;
    }
    public partial void Clear()
    {
        ClearRedos();
        var undos = Undos;
        Undos = new();
        foreach (var item in undos)
        {
            item.Cleanup(wasActionDone: true);
        }
        _IsUndoable.Value = false;
    }
    public partial void ClearRedos()
    {
        var redos = Redos;
        Redos = new();
        foreach (var item in redos)
        {
            item.Cleanup(wasActionDone: false);
        }
        _IsRedoable.Value = false;
    }
}