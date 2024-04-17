using Get.Data.Collections.Conversion;
using Get.Data.Collections.Linq;
using Get.Data.Collections.Update;
using System.Collections;

namespace Get.Data.Collections;
public class UpdateCollectionInitializer<T> : UpdateCollection<T>, IEnumerable<T>
{
    public void Add(T item) => (this as IUpdateCollection<T>).Add(item);
    public IEnumerator<T> GetEnumerator() => (this as IUpdateCollection<T>).AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
public class UpdateCollection<T> : IUpdateCollection<T>
{
    List<T> list = [];
    public T this[int index] {
        get => list[index];
        set => list[index] = value;
    }

    public int Count => list.Count;

    public event UpdateCollectionItemsChanged<T> ItemsChanged;

    public void Clear()
    {
        var oldList = list;
        list = [];
        ItemsChanged?.Invoke([new ItemsRemovedUpdateAction<T>(0, oldList.AsGDReadOnlyCollection())]);
    }

    public void Insert(int index, T item)
    {
        list.Insert(index, item);
        ItemsChanged?.Invoke([new ItemsAddedUpdateAction<T>(index, Collection.Single(item))]);
    }

    public void Move(int index1, int index2)
    {
        (list[index1], list[index2]) = (list[index2], list[index1]);
        ItemsChanged?.Invoke([new ItemsMovedUpdateAction<T>(index1, index2, list[index2], list[index1])]);
    }
    public void RemoveAt(int index)
    {
        var item = list[index];
        list.RemoveAt(index);
        ItemsChanged?.Invoke([new ItemsRemovedUpdateAction<T>(index, Collection.Single(item))]);
    }

}