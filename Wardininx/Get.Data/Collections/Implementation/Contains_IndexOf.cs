#nullable enable
namespace Get.Data.Collections.Implementation;

partial class Implementations
{
    public static bool Contains_IndexOf<T>(IGDCollection<T> collection, T item)
    {
        return collection.IndexOf(item) >= 0;
    }
}