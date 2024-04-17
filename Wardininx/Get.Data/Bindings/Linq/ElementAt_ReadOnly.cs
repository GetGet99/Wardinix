using Get.Data.Collections.Update;

namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IReadOnlyBinding<T> ElementAt<T>(this IUpdateReadOnlyCollection<T> collection, IReadOnlyBinding<int> index)
        => new ElementAtReadOnly<T>(collection, index);
}
class ElementAtReadOnly<T>(IUpdateReadOnlyCollection<T> collection, IReadOnlyBinding<int> index) : ElementAtBase<T?>(collection, index), IReadOnlyBinding<T?>
{
    public T? CurrentValue { get => value; }
}