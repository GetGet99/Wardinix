using Get.Data.Collections.Update;

namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IBinding<T> ElementAt<T>(this IUpdateFixedSizeCollection<T> collection, IReadOnlyBinding<int> index)
        => new ElementAt<T>(collection, index);
}
class ElementAt<T>(IUpdateFixedSizeCollection<T> collection, IReadOnlyBinding<int> index) : ElementAtBase<T?>(collection, index), IBinding<T?>
{
    public T? CurrentValue {
        get => value;
        set
        {
            var curIdx = index.CurrentValue;
            if (curIdx < 0 || curIdx >= collection.Count)
                throw new InvalidOperationException("Invalid Index");
            collection[index.CurrentValue] = value;
        }
    }
}