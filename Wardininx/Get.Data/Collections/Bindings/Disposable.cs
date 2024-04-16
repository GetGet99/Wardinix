namespace Get.Data.Collections;
readonly struct Disposable(Action OnDispose) : IDisposable
{
    public void Dispose() => OnDispose();
}