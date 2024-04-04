namespace Get.Data.Collections;
public interface IUpdateCollection<T> : IList<T>, ICollectionUpdateEvent<T>
{
    void Move(int index1, int index2);
}
public interface IReadOnlyUpdateCollection<T> : IReadOnlyList<T>, ICollectionUpdateEvent<T>
{
}
public interface ICollectionUpdateEvent<T>
{
    event UpdateCollectionItemsAdded<T> ItemsAdded;
    event UpdateCollectionItemsRemoved<T> ItemsRemoved;
    event UpdateCollectionItemsReplaced<T> ItemsReplaced;
    event UpdateCollectionItemsMoved<T> ItemsMoved;
}

public delegate void UpdateCollectionItemsAdded<T>(int startingIndex, IReadOnlyList<T> item);
public delegate void UpdateCollectionItemsRemoved<T>(int startingIndex, IReadOnlyList<T> item);
public delegate void UpdateCollectionItemsReplaced<T>(int index, T oldItem, T newItem);
public delegate void UpdateCollectionItemsMoved<T>(int oldIndex, int newIndex, T oldIndexItem, T newIndexItem);

abstract class CollectionUpdateEvent<T> : ICollectionUpdateEvent<T>
{

    public event UpdateCollectionItemsAdded<T> ItemsAdded
    {
        add
        {
            if (_ItemsAdded == null) RegisterItemsAddedEvent();
            _ItemsAdded += value;
        }
        remove
        {
            _ItemsAdded -= value;
            if (_ItemsAdded == null) UnregisterItemsAddedEvent();
        }
    }
    UpdateCollectionItemsAdded<T> _ItemsAdded;
    public event UpdateCollectionItemsRemoved<T> ItemsRemoved
    {
        add
        {
            if (_ItemsRemoved == null) RegisterItemsRemovedEvent();
            _ItemsRemoved += value;
        }
        remove
        {
            _ItemsRemoved -= value;
            if (_ItemsRemoved == null) UnregisterItemsRemovedEvent();
        }
    }
    UpdateCollectionItemsRemoved<T> _ItemsRemoved;
    public event UpdateCollectionItemsReplaced<T> ItemsReplaced
    {
        add
        {
            if (_ItemsReplaced == null) RegisterItemsReplacedEvent();
            _ItemsReplaced += value;
        }
        remove
        {
            _ItemsReplaced -= value;
            if (_ItemsReplaced == null) UnregisterItemsReplacedEvent();
        }
    }
    UpdateCollectionItemsReplaced<T> _ItemsReplaced;
    public event UpdateCollectionItemsMoved<T> ItemsMoved
    {
        add
        {
            if (_ItemsMoved == null) RegisterItemsMovedEvent();
            _ItemsMoved += value;
        }
        remove
        {
            _ItemsMoved -= value;
            if (_ItemsMoved == null) UnregisterItemsMovedEvent();
        }
    }
    UpdateCollectionItemsMoved<T> _ItemsMoved;
    protected void InvokeItemsAdded(int startingIndex, IReadOnlyList<T> item) => _ItemsAdded?.Invoke(startingIndex, item);
    protected void InvokeItemsRemoved(int startingIndex, IReadOnlyList<T> item) => _ItemsRemoved?.Invoke(startingIndex, item);
    protected void InvokeItemsReplaced(int index, T oldItem, T newItem) => _ItemsReplaced?.Invoke(index, oldItem, newItem);
    protected void InvokeItemsMoved(int oldIndex, int newIndex, T oldIndexItem, T newIndexItem) => _ItemsMoved?.Invoke(oldIndex, newIndex, oldIndexItem, newIndexItem);
    protected abstract void RegisterItemsAddedEvent();
    protected abstract void UnregisterItemsAddedEvent();
    protected abstract void RegisterItemsRemovedEvent();
    protected abstract void UnregisterItemsRemovedEvent();
    protected abstract void RegisterItemsReplacedEvent();
    protected abstract void UnregisterItemsReplacedEvent();
    protected abstract void RegisterItemsMovedEvent();
    protected abstract void UnregisterItemsMovedEvent();
}