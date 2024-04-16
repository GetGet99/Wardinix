#nullable enable
namespace Get.Data.Collections.Implementation;
partial class Implementations
{
    public static int IndexOf_LinearSearch<T>(IGDReadOnlyCollection<T> collection, T item, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        for (int i = 0; i < collection.Count; i++)
        {
            if (comparer.Equals(collection[i], item))
                return i;
        }
        return -1;
    }
}