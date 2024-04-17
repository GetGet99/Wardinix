using Get.Data.Collections.Update;

namespace Get.Data.Collections.Linq;
public static partial class Extension
{
    public static IUpdateCollection<T> Reverse<T>(this IUpdateCollection<T> c)
        => new ReverseUpdate<T>(c);
}
class ReverseUpdate<T>(IUpdateCollection<T> src) : ReverseUpdateBase<T>(src), IUpdateCollection<T>
{
    public T this[int index] { get => src[FlipIndex(index)]; set => src[FlipIndex(index)] = value; }

    public int Count => src.Count;

    public void Clear() => src.Clear();

    public void Insert(int index, T item)
    {
        if (src.Count == 0 || index == src.Count)
        {
            src.Insert(0, item);
        }
        else
        {
            src.Insert(FlipIndex(index), item);
        }
    }

    public void Move(int index1, int index2)
        => src.Move(FlipIndex(index1), FlipIndex(index2));

    public void RemoveAt(int index) => src.RemoveAt(FlipIndex(index));
}