using Get.Data.Collections;

namespace Get.Data.Collections.Linq;
public static partial class Extension
{
    public static IGDCollection<T> Reverse<T>(this IGDCollection<T> c)
        => new Reverse<T>(c);
}
readonly struct Reverse<T>(IGDCollection<T> c) : IGDCollection<T>
{
    int RealIndexAt(int index) => c.Count == 0 ? 0 : c.Count - 1 - index;
    public int Count => c.Count;
    public T this[int index] { get => c[RealIndexAt(index)]; set => c[RealIndexAt(index)] = value; }

    public void Insert(int index, T item)
    {
        if (c.Count == 0 || index == c.Count)
        {
            c.Insert(0, item);
        }
        else
        {
            c.Insert(RealIndexAt(index), item);
        }
    }
    public void RemoveAt(int index) => c.RemoveAt(RealIndexAt(index));
}