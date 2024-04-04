using Get.Data.Bindings;
using System.Collections;
namespace Get.Data.Collections;
class TwoWayConverterCollection<TDest, TSrc>(IUpdateCollection<TSrc> src, ForwardConverter<TSrc, TDest> forward, BackwardConverter<TSrc, TDest> backward) : CollectionUpdateEvent<TDest>, IUpdateCollection<TDest>
{
    public void CopyTo(TDest[] array, int arrayIndex)
    {
        TSrc[] srcArray = new TSrc[array.Length];
        src.CopyTo(srcArray, arrayIndex);
        for (int i = arrayIndex; i < array.Length; i++)
            array[i] = forward(srcArray[i]);
    }
    public TDest this[int index]
    {
        get => forward(src[index]);
        set => src[index] = backward(value);
    }
    public int Count => src.Count;
    public bool IsReadOnly => src.IsReadOnly;
    public void Add(TDest item)
        => src.Add(backward(item));

    public void Clear()
        => src.Clear();

    public bool Contains(TDest item)
        => src.Contains(backward(item));

    public int IndexOf(TDest item)
        => src.IndexOf(backward(item));

    public void Insert(int index, TDest item)
        => src.Insert(index, backward(item));

    public bool Remove(TDest item)
        => src.Remove(backward(item));

    public void RemoveAt(int index)
        => src.RemoveAt(index);

    public IEnumerator<TDest> GetEnumerator()
        => src.Select(x => forward(x)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    readonly struct SimpleListConvertWrapper(IReadOnlyList<TSrc> src, ForwardConverter<TSrc, TDest> forward) : IReadOnlyList<TDest>
    {
        public TDest this[int index] => forward(src[index]);

        public int Count => src.Count;

        Func<TSrc, TDest> ForwardAsFunc(ForwardConverter<TSrc, TDest> forward) => x => forward(x);
        public IEnumerator<TDest> GetEnumerator() => src.Select(ForwardAsFunc(forward)).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    protected override void RegisterItemsAddedEvent() => src.ItemsAdded += Source_ItemsAdded;
    protected override void UnregisterItemsAddedEvent() => src.ItemsAdded -= Source_ItemsAdded;
    private void Source_ItemsAdded(int startingIndex, IReadOnlyList<TSrc> item)
        => InvokeItemsAdded(startingIndex, new SimpleListConvertWrapper(item, forward));
    protected override void RegisterItemsRemovedEvent() => src.ItemsRemoved += Source_ItemsRemoved;
    protected override void UnregisterItemsRemovedEvent() => src.ItemsRemoved -= Source_ItemsRemoved;
    private void Source_ItemsRemoved(int startingIndex, IReadOnlyList<TSrc> item)
        => InvokeItemsRemoved(startingIndex, new SimpleListConvertWrapper(item, forward));
    protected override void RegisterItemsReplacedEvent() => src.ItemsReplaced += Source_ItemsReplaced;
    protected override void UnregisterItemsReplacedEvent() => src.ItemsReplaced -= Source_ItemsReplaced;
    private void Source_ItemsReplaced(int index, TSrc oldItem, TSrc newItem)
        => InvokeItemsReplaced(index, forward(oldItem), forward(newItem));
    protected override void RegisterItemsMovedEvent() => src.ItemsMoved += Source_ItemsMoved;
    protected override void UnregisterItemsMovedEvent() => src.ItemsMoved -= Source_ItemsMoved;
    private void Source_ItemsMoved(int oldIndex, int newIndex, TSrc oldIndexItem, TSrc newIndexItem)
        => InvokeItemsMoved(oldIndex, newIndex, forward(oldIndexItem), forward(newIndexItem));

    public void Move(int index1, int index2)
    {
        src.Move(index1, index2);
    }
}
class OneWayConverterCollection<TDest, TSrc>(IReadOnlyUpdateCollection<TSrc> src, ForwardConverter<TSrc, TDest> forward) : CollectionUpdateEvent<TDest>, IReadOnlyUpdateCollection<TDest>
{
    public TDest this[int index]
    {
        get => forward(src[index]);
    }
    public int Count => src.Count;
    public IEnumerator<TDest> GetEnumerator()
        => src.Select(x => forward(x)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    readonly struct SimpleListConvertWrapper(IReadOnlyList<TSrc> src, ForwardConverter<TSrc, TDest> forward) : IReadOnlyList<TDest>
    {
        public TDest this[int index] => forward(src[index]);

        public int Count => src.Count;

        Func<TSrc, TDest> ForwardAsFunc(ForwardConverter<TSrc, TDest> forward) => x => forward(x);
        public IEnumerator<TDest> GetEnumerator() => src.Select(ForwardAsFunc(forward)).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    protected override void RegisterItemsAddedEvent() => src.ItemsAdded += Source_ItemsAdded;
    protected override void UnregisterItemsAddedEvent() => src.ItemsAdded -= Source_ItemsAdded;
    private void Source_ItemsAdded(int startingIndex, IReadOnlyList<TSrc> item)
        => InvokeItemsAdded(startingIndex, new SimpleListConvertWrapper(item, forward));
    protected override void RegisterItemsRemovedEvent() => src.ItemsRemoved += Source_ItemsRemoved;
    protected override void UnregisterItemsRemovedEvent() => src.ItemsRemoved -= Source_ItemsRemoved;
    private void Source_ItemsRemoved(int startingIndex, IReadOnlyList<TSrc> item)
        => InvokeItemsRemoved(startingIndex, new SimpleListConvertWrapper(item, forward));
    protected override void RegisterItemsReplacedEvent() => src.ItemsReplaced += Source_ItemsReplaced;
    protected override void UnregisterItemsReplacedEvent() => src.ItemsReplaced -= Source_ItemsReplaced;
    private void Source_ItemsReplaced(int index, TSrc oldItem, TSrc newItem)
        => InvokeItemsReplaced(index, forward(oldItem), forward(newItem));
    protected override void RegisterItemsMovedEvent() => src.ItemsMoved += Source_ItemsMoved;
    protected override void UnregisterItemsMovedEvent() => src.ItemsMoved -= Source_ItemsMoved;
    private void Source_ItemsMoved(int oldIndex, int newIndex, TSrc oldIndexItem, TSrc newIndexItem)
        => InvokeItemsMoved(oldIndex, newIndex, forward(oldIndexItem), forward(newIndexItem));
}