#nullable enable
namespace Wardininx.UndoRedos;
partial class UndoManager
{
    
    public IDisposable OpenManipulativeTransform()
    {
        ShouldNotifyTransformActions = false;
        return new Disposable(() =>
        {
            ManipulativeTransformCompleted?.Invoke();
            ShouldNotifyTransformActions = true;
        });
    }
}