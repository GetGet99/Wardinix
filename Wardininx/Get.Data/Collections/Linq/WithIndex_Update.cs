using Get.Data.Collections;
using Get.Data.Collections.Update;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IUpdateCollection<IndexItem<T>> WithIndex<T>(this IUpdateCollection<T> c)
        => new WithIndexUpdate<T>(c);
}

class WithIndexUpdate<T>(IUpdateCollection<T> c) : WithIndexUpdateBase<T>(c), IUpdateCollection<IndexItem<T>>
{
    public IndexItem<T> this[int index]
    {
        get => new(index, c[index]);
        set => c[index] = value.Value;
    }
    public int Count => c.Count;

    public void Clear() => c.Clear();

    public void Insert(int index, IndexItem<T> item)
        => c.Insert(index, item.Value);

    public void Move(int index1, int index2) => c.Move(index1, index2);

    public void RemoveAt(int index)
        => c.RemoveAt(index);
}