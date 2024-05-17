namespace Wardininx.UndoRedos;

public delegate void UndoDelegate();
public delegate void RedoDelegate();
public delegate void CleanupDelegate(bool wasActionDone);
public delegate void UndoDelegate<T>(T value);
public delegate void RedoDelegate<T>(T value);
public delegate void CleanupDelegate<T>(T value, bool wasActionDone);