#nullable enable
namespace Get.Data.Collections.Implementation;

partial class Implementations
{
    public static void Clear_RemoveLast<T>(IGDCollection<T> collection)
    {
        while (collection.Count > 0) collection.RemoveAt(collection.Count - 1);
    }
}