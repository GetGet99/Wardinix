using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IGDCollection<IndexItem<T>> WithIndex<T>(this IGDCollection<T> c)
        => new WithIndex<T>(c);
}

readonly struct WithIndex<T>(IGDCollection<T> c) : IGDCollection<IndexItem<T>>
{
    public IndexItem<T> this[int index]
    {
        get => new(index, c[index]);
        set => c[index] = value.Value;
    }
    public int Count => c.Count;

    public void Insert(int index, IndexItem<T> item)
        => c.Insert(index, item.Value);

    public void RemoveAt(int index)
        => c.RemoveAt(index);
}