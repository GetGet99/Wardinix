using Get.Data.Collections;
using System.Collections;

namespace Get.Data.Collections.Conversion;
readonly struct GDReadOnlyListImpl<T>(IGDReadOnlyCollection<T> collection) : IReadOnlyList<T>
{
    public T this[int index] { get => collection[index]; }
    public int Count => collection.Count;
    public IEnumerator<T> GetEnumerator()
    { for (int i = 0; i < collection.Count; i++) yield return collection[i]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
readonly struct ReadOnlyListImpl<T>(IReadOnlyList<T> values) : IGDReadOnlyCollection<T>
{
    public T this[int index] => values[index];
    public int Count => values.Count;
}
partial class Extension
{
    public static IReadOnlyList<T> AsIReadOnlyList<T>(this IGDReadOnlyCollection<T> collection)
        => new GDReadOnlyListImpl<T>(collection);
    public static IGDReadOnlyCollection<T> AsGDReadOnlyCollection<T>(this IReadOnlyList<T> readOnlyList)
        => new ReadOnlyListImpl<T>(readOnlyList);
}