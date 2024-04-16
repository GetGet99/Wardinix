using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IGDFixedSizeCollection<IndexItem<T>> WithIndex<T>(this IGDFixedSizeCollection<T> c)
        => new WithIndexFixedSize<T>(c);
}

readonly struct WithIndexFixedSize<T>(IGDFixedSizeCollection<T> c) : IGDFixedSizeCollection<IndexItem<T>>
{
    public IndexItem<T> this[int index]
    {
        get => new(index, c[index]);
        set => c[index] = value.Value;
    }
    public int Count => c.Count;
}