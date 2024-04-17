using Get.Data.Collections;

namespace Get.Data.Collections.Linq;
public static partial class Extension
{
    public static IGDFixedSizeCollection<T> Reverse<T>(this IGDFixedSizeCollection<T> c)
        => new ReverseFixedSize<T>(c);
}
readonly struct ReverseFixedSize<T>(IGDFixedSizeCollection<T> c) : IGDFixedSizeCollection<T>
{
    int RealIndexAt(int index) => c.Count - index - 1;
    public int Count => c.Count;
    public T this[int index] { get => c[RealIndexAt(index)]; set => c[RealIndexAt(index)] = value; }
}