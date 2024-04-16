using Get.Data.Collections;
using Get.Data.Collections.Update;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IUpdateReadOnlyCollection<IndexItem<T>> WithIndex<T>(this IUpdateReadOnlyCollection<T> c)
        => new WithIndexReadOnlyUpdate<T>(c);
}

class WithIndexReadOnlyUpdate<T>(IUpdateReadOnlyCollection<T> c) : WithIndexUpdateBase<T>(c), IUpdateReadOnlyCollection<IndexItem<T>>
{
    public IndexItem<T> this[int index]
    {
        get => new(index, c[index]);
    }
    public int Count => c.Count;
}