using Get.Data.Collections.Conversion;
using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static List<T> EvalList<T>(this IGDReadOnlyCollection<T> c)
    {
        List<T> values = new(c.Count);
        for (int i = 0; i < c.Count; i++)
        {
            values[i] = c[i];
        }
        return values;
    }
    public static T[] EvalArray<T>(this IGDReadOnlyCollection<T> c)
    {
        T[] values = new T[c.Count];
        for (int i = 0; i < c.Count; i++)
        {
            values[i] = c[i];
        }
        return values;
    }
    public static IGDFixedSizeCollection<T> EvalFixedSize<T>(this IGDReadOnlyCollection<T> c)
        => c.EvalArray().AsGDFixedSizeCollection();
    public static IGDCollection<T> Eval<T>(this IGDReadOnlyCollection<T> c)
        => c.EvalList().AsGDCollection();
}

