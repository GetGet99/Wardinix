#nullable enable
namespace Get.Data.Collections.Implementation;

partial class Implementations
{
    public static void Clear_RemoveFirst<T>(IGDCollection<T> collection)
    {
        while (collection.Count > 0) collection.RemoveAt(0);
    }
}