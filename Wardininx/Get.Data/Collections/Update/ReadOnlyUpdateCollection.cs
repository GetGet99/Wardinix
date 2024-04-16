namespace Get.Data.Collections.Update;

class ReadOnlyUpdateCollection<T>(IUpdateCollection<T> src) : CollectionUpdateEvent<T>, IUpdateReadOnlyCollection<T>
{
    public T this[int index] => src[index];
    public int Count => src.Count;
    protected override void RegisterItemsChangedEvent()
        => src.ItemsChanged += InvokeItemsChanged;
    protected override void UnregisterItemsChangedEvent()
        => src.ItemsChanged -= InvokeItemsChanged;
}