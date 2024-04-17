using Get.Data.Collections.Update;

namespace Get.Data.Collections.Linq;
public static partial class Extension
{
    public static IUpdateFixedSizeCollection<T> Reverse<T>(this IUpdateFixedSizeCollection<T> c)
        => new ReverseUpdateFixedSize<T>(c);
}
class ReverseUpdateFixedSize<T>(IUpdateFixedSizeCollection<T> src) : ReverseUpdateBase<T>(src), IUpdateFixedSizeCollection<T>
{
    public T this[int index] { get => src[FlipIndex(index)]; set => src[FlipIndex(index)] = value; }

    public int Count => src.Count;
}