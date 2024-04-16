using Get.Data.Collections;

namespace Get.Data.Collections.Linq;
public static partial class Extension
{
    public static IGDCollection<TOut> Select<TIn, TOut>(this IGDCollection<TIn> c, ForwardConverter<TIn, TOut> f, BackwardConverter<TIn, TOut> b)
        => new Select<TIn, TOut>(c, f, b);
}
readonly struct Select<TIn, TOut>(IGDCollection<TIn> c, ForwardConverter<TIn, TOut> f, BackwardConverter<TIn, TOut> b) : IGDCollection<TOut>
{
    public TOut this[int index]
    {
        get => f(c[index]);
        set => c[index] = b(value);
    }
    public int Count => c.Count;

    public void Insert(int index, TOut item)
        => c.Insert(index, b(item));

    public void RemoveAt(int index)
        => c.RemoveAt(index);
}