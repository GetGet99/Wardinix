using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IGDReadOnlyCollection<IndexItem<T>> WithIndex<T>(this IGDReadOnlyCollection<T> c)
        => new WithIndexReadOnly<T>(c);
}

readonly struct WithIndexReadOnly<T>(IGDReadOnlyCollection<T> c) : IGDReadOnlyCollection<IndexItem<T>>
{
    public IndexItem<T> this[int index] => new(index, c[index]);
    public int Count => c.Count;
}