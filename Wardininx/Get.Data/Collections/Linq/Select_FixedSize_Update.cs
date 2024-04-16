using Get.Data.Collections.Update;
namespace Get.Data.Collections.Linq;
public static partial class Extension
{
    public static IUpdateFixedSizeCollection<TOut> Select<TIn, TOut>(this IUpdateFixedSizeCollection<TIn> c, ForwardConverter<TIn, TOut> f, BackwardConverter<TIn, TOut> b)
        => new SelectFixedSizeUpdate<TIn, TOut>(c, f, b);
}
class SelectFixedSizeUpdate<TSrc, TDest>(IUpdateFixedSizeCollection<TSrc> src, ForwardConverter<TSrc, TDest> forward, BackwardConverter<TSrc, TDest> backward) : SelectUpdateBase<TSrc, TDest>(src, forward), IUpdateFixedSizeCollection<TDest>
{
    public TDest this[int index]
    {
        get => forward(src[index]);
        set => src[index] = backward(value);
    }
    public int Count => src.Count;
}