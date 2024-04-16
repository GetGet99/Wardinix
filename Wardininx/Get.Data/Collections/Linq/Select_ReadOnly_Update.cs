using Get.Data.Collections.Update;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IUpdateReadOnlyCollection<TOut> Select<TIn, TOut>(this IUpdateReadOnlyCollection<TIn> c, ForwardConverter<TIn, TOut> f)
        => new SelectReadOnlyUpdate<TIn, TOut>(c, f);
}
class SelectReadOnlyUpdate<TSrc, TDest>(IUpdateReadOnlyCollection<TSrc> src, ForwardConverter<TSrc, TDest> forward) : SelectUpdateBase<TSrc, TDest>(src, forward), IUpdateReadOnlyCollection<TDest>
{
    public TDest this[int index]
    {
        get => forward(src[index]);
    }
    public int Count => src.Count;
}