using System.Collections;

namespace Get.Data.Collections;

public class UpdateCollection<T> : IUpdateCollection<T>
{
    List<T> list = [];
    public T this[int index] {
        get => list[index];
        set => list[index] = value;
    }

    public int Count => list.Count;

    public bool IsReadOnly => false;

    public event UpdateCollectionItemsAdded<T> ItemsAdded;
    public event UpdateCollectionItemsRemoved<T> ItemsRemoved;
    public event UpdateCollectionItemsReplaced<T> ItemsReplaced;
    public event UpdateCollectionItemsMoved<T> ItemsMoved;

    public void Add(T item)
    {
        list.Add(item);
        ItemsAdded?.Invoke(list.Count - 1, [item]);
    }

    public void Clear()
    {
        var oldList = list;
        list = [];
        ItemsRemoved?.Invoke(0, oldList);
    }

    public bool Contains(T item) => list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    public int IndexOf(T item) => list.IndexOf(item);

    public void Insert(int index, T item)
    {
        list.Insert(index, item);
        ItemsAdded?.Invoke(index, [item]);
    }

    public void Move(int index1, int index2)
    {
        (list[index1], list[index2]) = (list[index2], list[index1]);
        ItemsMoved?.Invoke(index1, index2, list[index2], list[index1]);
    }

    public bool Remove(T item)
    {
        var output = list.Remove(item);
        if (output)
            ItemsRemoved?.Invoke(list.Count, [item]);
        return output;
    }

    public void RemoveAt(int index)
    {
        var item = list[index];
        list.RemoveAt(index);
        ItemsRemoved?.Invoke(index, [item]);
    }

    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
}
class ReadOnlyUpdateCollection<T>(IUpdateCollection<T> src) : CollectionUpdateEvent<T>, IReadOnlyUpdateCollection<T>
{
    public T this[int index] => src[index];
    public int Count => src.Count;
    public IEnumerator<T> GetEnumerator()
        => src.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    protected override void RegisterItemsAddedEvent() => src.ItemsAdded += Source_ItemsAdded;
    protected override void UnregisterItemsAddedEvent() => src.ItemsAdded -= Source_ItemsAdded;
    private void Source_ItemsAdded(int startingIndex, IReadOnlyList<T> item)
        => InvokeItemsAdded(startingIndex, item);
    protected override void RegisterItemsRemovedEvent() => src.ItemsRemoved += Source_ItemsRemoved;
    protected override void UnregisterItemsRemovedEvent() => src.ItemsRemoved -= Source_ItemsRemoved;
    private void Source_ItemsRemoved(int startingIndex, IReadOnlyList<T> item)
        => InvokeItemsRemoved(startingIndex, item);
    protected override void RegisterItemsReplacedEvent() => src.ItemsReplaced += Source_ItemsReplaced;
    protected override void UnregisterItemsReplacedEvent() => src.ItemsReplaced -= Source_ItemsReplaced;
    private void Source_ItemsReplaced(int index, T oldItem, T newItem)
        => InvokeItemsReplaced(index, oldItem, newItem);
    protected override void RegisterItemsMovedEvent() => src.ItemsMoved += Source_ItemsMoved;
    protected override void UnregisterItemsMovedEvent() => src.ItemsMoved -= Source_ItemsMoved;
    private void Source_ItemsMoved(int oldIndex, int newIndex, T oldIndexItem, T newIndexItem)
        => InvokeItemsMoved(oldIndex, newIndex, oldIndexItem, newIndexItem);
}