#nullable enable
namespace Get.Data.Collections;

partial class DefaultImplementations
{
    public static bool Remove<T>(this IGDCollection<T> collection, T item)
    {
        var idx = collection.IndexOf(item);
        if (idx >= 0)
            collection.RemoveAt(idx);
        return idx >= 0;
    }
}