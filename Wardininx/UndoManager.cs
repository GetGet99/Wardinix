using Get.Data.Properties;

namespace Wardininx.UndoRedos;

public delegate void UndoDelegate();
public delegate void RedoDelegate();
public delegate void CleanupDelegate(bool wasActionDone);
record class UndoableAction(string ActionName, UndoDelegate Undo, RedoDelegate Redo, CleanupDelegate Cleanup);
public delegate void UndoDelegate<T>(T value);
public delegate void RedoDelegate<T>(T value);
public delegate void CleanupDelegate<T>(T value, bool wasActionDone);
record class UndoableAction<T>(string ActionName, T Value, UndoDelegate<T> UndoParam, RedoDelegate<T> RedoParam, CleanupDelegate<T> CleanupParam) :
    UndoableAction(ActionName, () => UndoParam(Value), () => RedoParam(Value), x => CleanupParam(Value, x));
class UndoManager
{
    
    LinkedList<UndoableAction> Undos = new(), Redos = new();
    readonly Property<bool> _IsUndoable = new(false), _IsRedoable = new(false);
    public ReadOnlyProperty<bool> IsUndoableProperty { get; }
    public ReadOnlyProperty<bool> IsRedoableProperty { get; }
    public UndoManager()
    {
        IsUndoableProperty = new(_IsUndoable);
        IsRedoableProperty = new(_IsRedoable);
    }
    public void Undo()
    {
        if (!_IsUndoable.Value) throw new InvalidOperationException();
        var last = Undos.Last.Value;
        Undos.RemoveLast();
        Redos.AddFirst(last);
        last.Undo();
        _IsUndoable.Value = Undos.Count is not 0;
        _IsRedoable.Value = true;
    }
    public void Redo()
    {
        if (!_IsRedoable.Value) throw new InvalidOperationException();
        var first = Redos.First.Value;
        Undos.AddLast(first);
        Redos.RemoveFirst();
        first.Redo();
        _IsUndoable.Value = true;
        _IsRedoable.Value = Redos.Count is not 0;
    }
    public void Clear()
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
    public void ClearRedos()
    {
        var redos = Redos;
        Redos = new();
        foreach (var item in redos)
        {
            item.Cleanup(wasActionDone: false);
        }
        _IsRedoable.Value = false;
    }
    public void AddAction(UndoableAction action)
    {
        if (Redos.Count > 0) ClearRedos();
        Undos.AddLast(action);
        _IsUndoable.Value = true;
    }
    public void DoAndAddAction(UndoableAction action)
    {
        action.Redo();
        AddAction(action);
    }
}