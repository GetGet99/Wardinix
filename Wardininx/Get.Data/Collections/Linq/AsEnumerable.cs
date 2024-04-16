using Get.Data.Collections.Conversion;
using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IEnumerable<T> AsEnumerable<T>(this IGDReadOnlyCollection<T> c)
    {
        for (int i = 0; i < c.Count; i++) 
            yield return c[i];
    }
}

