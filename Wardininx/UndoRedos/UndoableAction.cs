namespace Wardininx.UndoRedos;
public record class UndoableAction(string ActionName, UndoDelegate Undo, RedoDelegate Redo, CleanupDelegate Cleanup);
record class UndoableAction<T>(string ActionName, T Value, UndoDelegate<T> UndoParam, RedoDelegate<T> RedoParam, CleanupDelegate<T> CleanupParam) :
    UndoableAction(ActionName, () => UndoParam(Value), () => RedoParam(Value), x => CleanupParam(Value, x));