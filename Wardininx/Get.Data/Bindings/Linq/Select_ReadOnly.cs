using Get.Data.Collections.Update;

namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IReadOnlyBinding<TDest> Select<TSrc, TDest>(this IReadOnlyBinding<TSrc> src, ForwardConverter<TSrc, TDest> forwardConverter)
        => new SelectReadOnly<TSrc, TDest>(src, forwardConverter);
}
class SelectReadOnly<TIn, TOut>(IReadOnlyBinding<TIn> inBinding, ForwardConverter<TIn, TOut> converter) : SelectBase<TIn, TOut>(inBinding, converter), IReadOnlyBinding<TOut>
{
    public TOut CurrentValue => _value;
}