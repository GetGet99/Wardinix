using Get.Data.Properties;
using Get.Data.XACL;
using System.Collections;
using System.Reflection;

namespace Get.Data.Collections;

class TwoWayUpdateCollectionProperty<T> : Property<IUpdateCollection<T>>, IUpdateCollection<T>
{
    readonly IUpdateCollection<T> currentValue = new UpdateCollection<T>();
    public TwoWayUpdateCollectionProperty() : base(new UpdateCollection<T>())
    {
        ValueChanged += UpdateCollectionProperty_ValueChanged;
        ItemsAdded += TwoWayUpdateCollectionProperty_ItemsAdded;
        ItemsRemoved += TwoWayUpdateCollectionProperty_ItemsRemoved;
        ItemsReplaced += TwoWayUpdateCollectionProperty_ItemsReplaced;
        ItemsMoved += TwoWayUpdateCollectionProperty_ItemsMoved;
    }

    private void TwoWayUpdateCollectionProperty_ItemsMoved(int oldIndex, int newIndex, T oldIndexItem, T newIndexItem)
    {
        if (!modifying)
        {
            modifying = true;
            Value.Move(oldIndex, newIndex);
            modifying = false;
        }
    }

    private void TwoWayUpdateCollectionProperty_ItemsReplaced(int index, T oldItem, T newItem)
    {
        if (!modifying)
        {
            modifying = true;
            Value[index] = newItem;
            modifying = false;
        }
    }

    private void TwoWayUpdateCollectionProperty_ItemsRemoved(int startingIndex, IReadOnlyList<T> item)
    {
        if (!modifying)
        {
            modifying = true;
            for (int i = 0; i < item.Count; i++)
                Value.RemoveAt(startingIndex);
            modifying = false;
        }
    }

    private void TwoWayUpdateCollectionProperty_ItemsAdded(int startingIndex, IReadOnlyList<T> item)
    {
        if (!modifying)
        {
            modifying = true;
            for (int i = 0; i < item.Count; i++)
                Value.Insert(startingIndex + i, item[i]);
            modifying = false;
        }
    }

    private void UpdateCollectionProperty_ValueChanged(IUpdateCollection<T> oldValue, IUpdateCollection<T> newValue)
    {
        oldValue.ItemsAdded -= Value_ItemsAdded;
        oldValue.ItemsRemoved -= Value_ItemsRemoved;
        oldValue.ItemsReplaced -= Value_ItemsReplaced;
        oldValue.ItemsMoved -= Value_ItemsMoved;
        modifying = true;
        currentValue.Clear();
        if (currentValue.Count > 0) throw new InvalidOperationException();
        foreach (var newItem in newValue)
            currentValue.Add(newItem);
        modifying = false;
        newValue.ItemsAdded += Value_ItemsAdded;
        newValue.ItemsRemoved += Value_ItemsRemoved;
        newValue.ItemsReplaced += Value_ItemsReplaced;
        newValue.ItemsMoved += Value_ItemsMoved;
    }
    bool modifying = false;
    private void Value_ItemsMoved(int index1, int index2, T item1, T item2)
    {
        if (modifying) return;
        modifying = true;
        (currentValue[index1], currentValue[index2]) = (currentValue[index2], currentValue[index1]);
        modifying = false;
    }

    private void Value_ItemsReplaced(int index, T oldItem, T newItem)
    {
        if (modifying) return;
        modifying = true;
        currentValue[index] = newItem;
        modifying = false;
    }

    private void Value_ItemsRemoved(int startingIndex, IReadOnlyList<T> item)
    {
        if (modifying) return;
        modifying = true;
        for (int i = 0; i < item.Count; i++)
            currentValue.RemoveAt(startingIndex);
        modifying = false;
    }

    private void Value_ItemsAdded(int startingIndex, IReadOnlyList<T> item)
    {
        if (modifying) return;
        modifying = true;
        for (int i = 0; i < item.Count; i++)
            currentValue.Insert(startingIndex + i, item[i]);
        modifying = false;
    }

    public T this[int index] { get => ((IList<T>)currentValue)[index]; set => ((IList<T>)currentValue)[index] = value; }

    public int Count => currentValue.Count;

    public bool IsReadOnly => currentValue.IsReadOnly;

    public event UpdateCollectionItemsAdded<T> ItemsAdded
    {
        add
        {
            currentValue.ItemsAdded += value;
        }

        remove
        {
            currentValue.ItemsAdded -= value;
        }
    }

    public event UpdateCollectionItemsRemoved<T> ItemsRemoved
    {
        add
        {
            currentValue.ItemsRemoved += value;
        }

        remove
        {
            currentValue.ItemsRemoved -= value;
        }
    }

    public event UpdateCollectionItemsReplaced<T> ItemsReplaced
    {
        add
        {
            currentValue.ItemsReplaced += value;
        }

        remove
        {
            currentValue.ItemsReplaced -= value;
        }
    }

    public event UpdateCollectionItemsMoved<T> ItemsMoved
    {
        add
        {
            currentValue.ItemsMoved += value;
        }

        remove
        {
            currentValue.ItemsMoved -= value;
        }
    }

    public void Add(T item)
    {
        currentValue.Add(item);
    }

    public void Clear()
    {
        currentValue.Clear();
    }

    public bool Contains(T item)
    {
        return currentValue.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        currentValue.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return currentValue.GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return currentValue.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        currentValue.Insert(index, item);
    }

    public bool Remove(T item)
    {
        return currentValue.Remove(item);
    }

    public void RemoveAt(int index)
    {
        currentValue.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)currentValue).GetEnumerator();
    }

    public void Move(int index1, int index2)
    {
        currentValue.Move(index1, index2);
    }
}
class OneWayUpdateCollectionProperty<T> : Property<IReadOnlyUpdateCollection<T>>, IReadOnlyUpdateCollection<T>
{
    IUpdateCollection<T> currentValue = new UpdateCollection<T>();
    public OneWayUpdateCollectionProperty() : base(new UpdateCollection<T>().AsReadOnly())
    {
        ValueChanged += UpdateCollectionProperty_ValueChanged;
    }
    private void UpdateCollectionProperty_ValueChanged(IReadOnlyUpdateCollection<T> oldValue, IReadOnlyUpdateCollection<T> newValue)
    {
        oldValue.ItemsAdded -= Value_ItemsAdded;
        oldValue.ItemsRemoved -= Value_ItemsRemoved;
        oldValue.ItemsReplaced -= Value_ItemsReplaced;
        oldValue.ItemsMoved -= Value_ItemsMoved;
        currentValue.Clear();
        foreach (var newItem in newValue)
            currentValue.Add(newItem);
        newValue.ItemsAdded += Value_ItemsAdded;
        newValue.ItemsRemoved += Value_ItemsRemoved;
        newValue.ItemsReplaced += Value_ItemsReplaced;
        newValue.ItemsMoved += Value_ItemsMoved;
    }

    private void Value_ItemsMoved(int index1, int index2, T item1, T item2)
    {
        (currentValue[index1], currentValue[index2]) = (currentValue[index2], currentValue[index1]);
    }

    private void Value_ItemsReplaced(int index, T oldItem, T newItem)
    {
        currentValue[index] = newItem;
    }

    private void Value_ItemsRemoved(int startingIndex, IReadOnlyList<T> item)
    {
        for (int i = 0; i < item.Count; i++)
            currentValue.RemoveAt(startingIndex);
    }

    private void Value_ItemsAdded(int startingIndex, IReadOnlyList<T> item)
    {
        for (int i = 0; i < item.Count; i++)
            currentValue.Insert(startingIndex + i, item[i]);
    }

    public T this[int index] { get => ((IList<T>)currentValue)[index]; set => ((IList<T>)currentValue)[index] = value; }

    public int Count => currentValue.Count;


    public event UpdateCollectionItemsAdded<T> ItemsAdded
    {
        add
        {
            currentValue.ItemsAdded += value;
        }

        remove
        {
            currentValue.ItemsAdded -= value;
        }
    }

    public event UpdateCollectionItemsRemoved<T> ItemsRemoved
    {
        add
        {
            currentValue.ItemsRemoved += value;
        }

        remove
        {
            currentValue.ItemsRemoved -= value;
        }
    }

    public event UpdateCollectionItemsReplaced<T> ItemsReplaced
    {
        add
        {
            currentValue.ItemsReplaced += value;
        }

        remove
        {
            currentValue.ItemsReplaced -= value;
        }
    }

    public event UpdateCollectionItemsMoved<T> ItemsMoved
    {
        add
        {
            currentValue.ItemsMoved += value;
        }

        remove
        {
            currentValue.ItemsMoved -= value;
        }
    }
    public IEnumerator<T> GetEnumerator()
    {
        return currentValue.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)currentValue).GetEnumerator();
    }
}