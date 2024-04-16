using Get.Data.Collections;
using System.Collections;

namespace Get.Data.Collections.Conversion;
readonly struct ListImpl<T>(IList<T> values) : IGDCollection<T>
{
    public T this[int index]
    {
        get => values[index];
        set => values[index] = value;
    }

    public int Count => values.Count;

    public void Insert(int index, T item) => values.Insert(index, item);

    public void RemoveAt(int index) => values.RemoveAt(index);
}
readonly struct GDListImpl<T>(IGDCollection<T> collection) : IList<T>
{
    public T this[int index] { get => collection[index]; set => collection[index] = value; }
    public int Count => collection.Count;
    public bool IsReadOnly => false;
    public void Add(T item) => collection.Add(item);
    public void Clear() => collection.Clear();
    public bool Contains(T item) => collection.Contains(item);
    public IEnumerator<T> GetEnumerator()
    { for (int i = 0; i < collection.Count; i++) yield return collection[i]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int IndexOf(T item) => collection.IndexOf(item);
    public void Insert(int index, T item) => collection.Insert(index, item);
    public bool Remove(T item) => collection.Remove(item);
    public void RemoveAt(int index) => collection.RemoveAt(index);
    public void CopyTo(T[] array, int arrayIndex)
    {
        var last = Math.Min(array.Length - arrayIndex, collection.Count);
        for (int i = 0; i < last; i++)
            array[i + arrayIndex] = collection[i];
    }
}
partial class Extension
{
    public static IList<T> AsIList<T>(this IGDCollection<T> collection)
        => new GDListImpl<T>(collection);
    public static IGDCollection<T> AsGDCollection<T>(this IList<T> list)
        => new ListImpl<T>(list);
}