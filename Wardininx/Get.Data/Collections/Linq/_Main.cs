using Get.Data.Collections.Update;

namespace Get.Data.Collections.Linq;

public static partial class Extension
{
    public static IGDCollection<T> AsCollection<T>(this IGDCollection<T> c) => c;
    public static IGDReadOnlyCollection<T> AsReadOnly<T>(this IGDReadOnlyCollection<T> c) => c;
    public static IGDFixedSizeCollection<T> AsFixedSize<T>(this IGDFixedSizeCollection<T> c) => c;
    public static IUpdateCollection<T> AsUpdate<T>(this IUpdateCollection<T> c) => c;
    public static IUpdateReadOnlyCollection<T> AsUpdateReadOnly<T>(this IUpdateReadOnlyCollection<T> c) => c;
    public static IUpdateFixedSizeCollection<T> AsUpdateFixedSize<T>(this IUpdateFixedSizeCollection<T> c) => c;
}