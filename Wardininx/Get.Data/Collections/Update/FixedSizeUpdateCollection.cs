namespace Get.Data.Collections.Update;

class FixedSizeUpdateCollection<T>(IUpdateCollection<T> src) : CollectionUpdateEvent<T>, IUpdateFixedSizeCollection<T>
{
    public T this[int index]
    {
        get => src[index];
        set => src[index] = value;
    }
    public int Count => src.Count;
    protected override void RegisterItemsChangedEvent()
        => src.ItemsChanged += InvokeItemsChanged;
    protected override void UnregisterItemsChangedEvent()
        => src.ItemsChanged -= InvokeItemsChanged;
}