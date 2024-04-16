using Get.Data.Collections.Update;
namespace Get.Data.Collections;
public static partial class Extension
{
    public static IUpdateCollection<TOut> Select<TIn, TOut>(this IUpdateCollection<TIn> c, ForwardConverter<TIn, TOut> f, BackwardConverter<TIn, TOut> b)
        => new SelectUpdate<TIn, TOut>(c, f, b);
}
class SelectUpdate<TSrc, TDest>(IUpdateCollection<TSrc> src, ForwardConverter<TSrc, TDest> forward, BackwardConverter<TSrc, TDest> backward) : SelectUpdateBase<TSrc, TDest>(src, forward), IUpdateCollection<TDest>
{
    public TDest this[int index]
    {
        get => forward(src[index]);
        set => src[index] = backward(value);
    }
    public int Count => src.Count;
    public void Clear() => src.Clear();
    public void Insert(int index, TDest item) => src.Insert(index, backward(item));
    public void RemoveAt(int index) => src.RemoveAt(index);
    public void Move(int index1, int index2)
        => src.Move(index1, index2);
}