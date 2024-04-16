using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension {
    public static IGDReadOnlyCollection<TOut> Select<TIn, TOut>(this IGDReadOnlyCollection<TIn> c, ForwardConverter<TIn, TOut> f)
        => new SelectReadOnly<TIn, TOut>(c, f);
    public static IGDReadOnlyCollection<T> Select<T>(this IGDReadOnlyCollection<T> c, ForwardConverter<T, T> f)
        => new SelectReadOnly<T, T>(c, f);
}

readonly struct SelectReadOnly<TIn, TOut>(IGDReadOnlyCollection<TIn> c, ForwardConverter<TIn, TOut> f) : IGDReadOnlyCollection<TOut>
{
    public TOut this[int index] => f(c[index]);
    public int Count => c.Count;
}