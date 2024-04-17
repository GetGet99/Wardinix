namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IReadOnlyBinding<TDest> SelectPath<TSrc, TDest>(this IReadOnlyBinding<TSrc> src, Func<TSrc, IReadOnlyBinding<TDest>> selector)
        => new SelectPathReadOnly<TSrc, TDest>(src, selector);
}
class SelectPathReadOnly<TSrc, TDest>(IReadOnlyBinding<TSrc> src, Func<TSrc, IReadOnlyBinding<TDest>> selector) :
    SelectPathBase<TSrc, TDest, IReadOnlyBinding<TDest>>(src, selector), IReadOnlyBinding<TDest>
{
    public TDest CurrentValue { get => currentBinding.CurrentValue; }
}