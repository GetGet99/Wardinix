namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IReadOnlyBinding<TOut> Zip<TIn1, TIn2, TOut>(this IReadOnlyBinding<TIn1> inBinding1, IReadOnlyBinding<TIn2> inBinding2, ZipForwardConverter<TIn1, TIn2, TOut> converter)
        => new ZipReadOnly<TIn1, TIn2, TOut>(inBinding1, inBinding2, converter);
}
class ZipReadOnly<TIn1, TIn2, TOut>(IReadOnlyBinding<TIn1> inBinding1, IReadOnlyBinding<TIn2> inBinding2, ZipForwardConverter<TIn1, TIn2, TOut> converter)
    : ZipBase<TIn1, TIn2, TOut>(inBinding1, inBinding2, converter), IReadOnlyBinding<TOut>
{
    public TOut CurrentValue => _value;
}