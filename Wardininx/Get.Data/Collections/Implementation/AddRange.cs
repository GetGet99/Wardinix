#nullable enable
namespace Get.Data.Collections;

partial class DefaultImplementations
{
    public static void AddRange<T>(this IGDCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items) collection.Add(item);
    }
}