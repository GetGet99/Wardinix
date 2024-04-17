using Get.Data.Collections;

namespace Get.Data.Collections.Linq;
public static partial class Extension
{
    public static IGDReadOnlyCollection<T> Reverse<T>(this IGDReadOnlyCollection<T> c)
        => new ReverseReadOnly<T>(c);
}
readonly struct ReverseReadOnly<T>(IGDReadOnlyCollection<T> c) : IGDReadOnlyCollection<T>
{
    int RealIndexAt(int index) => c.Count - index - 1;
    public int Count => c.Count;
    public T this[int index] { get => c[RealIndexAt(index)]; }
}