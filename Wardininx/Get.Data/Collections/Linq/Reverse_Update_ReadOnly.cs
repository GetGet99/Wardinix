using Get.Data.Collections.Update;

namespace Get.Data.Collections.Linq;
public static partial class Extension
{
    public static IUpdateReadOnlyCollection<T> Reverse<T>(this IUpdateReadOnlyCollection<T> c)
        => new ReverseUpdateReadOnly<T>(c);
}
class ReverseUpdateReadOnly<T>(IUpdateReadOnlyCollection<T> src) : ReverseUpdateBase<T>(src), IUpdateReadOnlyCollection<T>
{
    public T this[int index] { get => src[FlipIndex(index)]; }

    public int Count => src.Count;
}