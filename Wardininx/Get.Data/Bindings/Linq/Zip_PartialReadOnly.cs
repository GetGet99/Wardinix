namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IBinding<TOut> Zip<TIn1, TIn2, TOut>(this IBinding<TIn1> inBinding1, IReadOnlyBinding<TIn2> inBinding2, ZipForwardConverter<TIn1, TIn2, TOut> converter, ZipPartialAdvancedBackwardConverter<TIn1, TIn2, TOut> backwardConverter)
        => new ZipPartialReadOnly<TIn1, TIn2, TOut>(inBinding1, inBinding2, converter, backwardConverter);
}
class ZipPartialReadOnly<TIn1, TIn2, TOut>(IBinding<TIn1> inBinding1, IReadOnlyBinding<TIn2> inBinding2, ZipForwardConverter<TIn1, TIn2, TOut> converter, ZipPartialAdvancedBackwardConverter<TIn1, TIn2, TOut> backwardConverter)
    : ZipBase<TIn1, TIn2, TOut>(inBinding1, inBinding2, converter), IBinding<TOut>
{
    public TOut CurrentValue
    {
        get => _value;
        set
        {
            var val1 = owner1;
            var val2 = owner2;
            backwardConverter(value, ref val1, val2);
            if (!EqualityComparer<TIn1>.Default.Equals(val1, inBinding1.CurrentValue))
                inBinding1.CurrentValue = val1;
        }
    }
}