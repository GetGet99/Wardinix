using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IEnumerable<IndexItem<T>> WithIndex<T>(this IEnumerable<T> c)
    {
        int i = 0;
        foreach (var item in c)
            yield return new IndexItem<T>(i++, item);
    }
}