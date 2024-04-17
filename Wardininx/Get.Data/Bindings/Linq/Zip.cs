namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IBinding<TOut> Zip<TIn1, TIn2, TOut>(this IBinding<TIn1> inBinding1, IBinding<TIn2> inBinding2, ZipForwardConverter<TIn1, TIn2, TOut> converter, ZipAdvancedBackwardConverter<TIn1, TIn2, TOut> backwardConverter)
        => new Zip<TIn1, TIn2, TOut>(inBinding1, inBinding2, converter, backwardConverter);
}
class Zip<TIn1, TIn2, TOut>(IBinding<TIn1> inBinding1, IBinding<TIn2> inBinding2, ZipForwardConverter<TIn1, TIn2, TOut> converter, ZipAdvancedBackwardConverter<TIn1, TIn2, TOut> backwardConverter)
    : ZipBase<TIn1, TIn2, TOut>(inBinding1, inBinding2, converter), IBinding<TOut>
{
    public TOut CurrentValue
    {
        get => _value;
        set
        {
            var val1 = owner1;
            var val2 = owner2;
            backwardConverter(value, ref val1, ref val2);
            if (!EqualityComparer<TIn1>.Default.Equals(val1, inBinding1.CurrentValue))
                inBinding1.CurrentValue = val1;
            if (!EqualityComparer<TIn2>.Default.Equals(val2, inBinding2.CurrentValue))
                inBinding2.CurrentValue = val2;
        }
    }
}