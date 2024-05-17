#nullable enable
namespace Wardininx.UndoRedos;
public partial class UndoManager
{
    public bool IsUndoable => IsUndoableProperty.CurrentValue;
    public bool IsRedoable => IsRedoableProperty.CurrentValue;
    public IReadOnlyProperty<bool> IsUndoableProperty { get; }
    public IReadOnlyProperty<bool> IsRedoableProperty { get; }
    public partial void Undo();
    public partial void Redo();
    public partial void Clear();
    public partial void ClearRedos();
    public partial UndoableActionGroup OpenGroup(string actionName);
    // Manipulative Transforms
    public bool ShouldNotifyTransformActions { get; private set; }
    public event Action? ManipulativeTransformCompleted;
}