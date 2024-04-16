using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IGDFixedSizeCollection<TOut> Select<TIn, TOut>(this IGDFixedSizeCollection<TIn> c, ForwardConverter<TIn, TOut> f, BackwardConverter<TIn, TOut> b)
        => new SelectFixedSize<TIn, TOut>(c, f, b);
}

readonly struct SelectFixedSize<TIn, TOut>(IGDFixedSizeCollection<TIn> c, ForwardConverter<TIn, TOut> f, BackwardConverter<TIn, TOut> b) : IGDFixedSizeCollection<TOut>
{
    public TOut this[int index]
    {
        get => f(c[index]);
        set => c[index] = b(value);
    }
    public int Count => c.Count;
}
