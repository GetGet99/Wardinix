#nullable enable
namespace Get.Data.Collections;

partial class DefaultImplementations
{
    public static void Add<T>(this IGDCollection<T> collection, T item)
        => collection.Insert(collection.Count, item);
}