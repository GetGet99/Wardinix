using System.Collections;

namespace Get.Data.Collections;
public record struct IndexItem<T>(int Index, T Value);
class IndexReadOnlyUpdateCollection<T>(IReadOnlyUpdateCollection<T> src) : CollectionUpdateEvent<IndexItem<T>>, IReadOnlyUpdateCollection<IndexItem<T>>
{
    public IndexItem<T> this[int index] => new(index, src[index]);

    public int Count => src.Count;

    public IEnumerator<IndexItem<T>> GetEnumerator()
    {
        int i = 0;
        foreach (var item in src)
            yield return new(i++, item);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected override void RegisterItemsAddedEvent() => src.ItemsAdded += Src_ItemsAdded;
    protected override void UnregisterItemsAddedEvent() => src.ItemsAdded -= Src_ItemsAdded;
    readonly struct PartialList(IReadOnlyList<T> original, int offset, int length) : IReadOnlyList<IndexItem<T>>
    {
        // Bad Security, negative index
        public IndexItem<T> this[int index] => new(index, original[index + offset]);

        public int Count => length;

        public IEnumerator<IndexItem<T>> GetEnumerator()
        {
            for (int i = offset; i < original.Count; i++)
                yield return new(i, original[i]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    private void Src_ItemsAdded(int startingIndex, IReadOnlyList<T> item)
    {
        int itemsAdded = item.Count;
        InvokeItemsAdded(startingIndex, new PartialList(item, 0, item.Count));
        // Sends update to everything else above that
        for (int i = startingIndex + item.Count - 1; i < src.Count; i++)
        {
            InvokeItemsReplaced(i, new(i - itemsAdded, src[i]), new(i, src[i]));
        }
    }

    protected override void RegisterItemsRemovedEvent() => src.ItemsRemoved += Src_ItemsRemoved;
    protected override void UnregisterItemsRemovedEvent() => src.ItemsRemoved -= Src_ItemsRemoved;

    private void Src_ItemsRemoved(int startingIndex, IReadOnlyList<T> item)
    {
        int itemsRemoved = item.Count;
        InvokeItemsRemoved(startingIndex, new PartialList(item, 0, item.Count));
        // Sends update to everything else above that
        for (int i = startingIndex; i < src.Count; i++)
        {
            InvokeItemsReplaced(i, new(i + itemsRemoved, src[i]), new(i, src[i]));
        }
    }

    protected override void RegisterItemsMovedEvent() => src.ItemsMoved += Src_ItemsMoved;
    protected override void UnregisterItemsMovedEvent() => src.ItemsMoved -= Src_ItemsMoved;
    private void Src_ItemsMoved(int oldIndex, int newIndex, T oldIndexItem, T newIndexItem)
        => InvokeItemsMoved(oldIndex, newIndex, new(oldIndex, oldIndexItem), new(newIndex, newIndexItem));

    protected override void RegisterItemsReplacedEvent() => src.ItemsReplaced += Src_ItemsReplaced;
    protected override void UnregisterItemsReplacedEvent() => src.ItemsReplaced -= Src_ItemsReplaced;
    private void Src_ItemsReplaced(int index, T oldItem, T newItem)
        => InvokeItemsReplaced(index, new(index, oldItem), new(index, newItem));
}