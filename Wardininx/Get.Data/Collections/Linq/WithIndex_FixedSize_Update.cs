using Get.Data.Collections;
using Get.Data.Collections.Update;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IUpdateFixedSizeCollection<IndexItem<T>> WithIndex<T>(this IUpdateFixedSizeCollection<T> c)
        => new WithIndexFixedSizeUpdate<T>(c);
}

class WithIndexFixedSizeUpdate<T>(IUpdateFixedSizeCollection<T> c) : WithIndexUpdateBase<T>(c), IUpdateFixedSizeCollection<IndexItem<T>>
{
    public IndexItem<T> this[int index]
    {
        get => new(index, c[index]);
        set => c[index] = value.Value;
    }
    public int Count => c.Count;
}