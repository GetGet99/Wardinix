namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IBinding<TDest> SelectPath<TSrc, TDest>(this IReadOnlyBinding<TSrc> src, Func<TSrc, IBinding<TDest>> selector)
        => new SelectPath<TSrc, TDest>(src, selector);
}
class SelectPath<TSrc, TDest>(IReadOnlyBinding<TSrc> src, Func<TSrc, IBinding<TDest>> selector) :
    SelectPathBase<TSrc, TDest, IBinding<TDest>>(src, selector), IBinding<TDest>
{
    public TDest CurrentValue { get => currentBinding.CurrentValue; set => currentBinding.CurrentValue = value; }
}