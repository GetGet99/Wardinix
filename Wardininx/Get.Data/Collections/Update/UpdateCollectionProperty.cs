using Get.Data.Collections.Linq;
using Get.Data.Collections.Update;
using Get.Data.Properties;
using System.Collections;

namespace Get.Data.Collections;


class TwoWayUpdateCollectionProperty<T> : UpdateCollectionPropertyBase<T, IUpdateCollection<T>>, IUpdateCollection<T>
{
    public TwoWayUpdateCollectionProperty() : base(new UpdateCollection<T>())
    {

    }
    public T this[int index] { get => CurrentValue[index]; set => CurrentValue[index] = value; }

    public int Count => CurrentValue.Count;
    public void Clear()
    {
        CurrentValue.Clear();
    }

    public void Insert(int index, T item)
    {
        CurrentValue.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        CurrentValue.RemoveAt(index);
    }
    public void Move(int index1, int index2)
    {
        CurrentValue.Move(index1, index2);
    }
}
class OneWayUpdateCollectionProperty<T> : UpdateCollectionPropertyBase<T, IUpdateReadOnlyCollection<T>>, IUpdateReadOnlyCollection<T>
{
    public OneWayUpdateCollectionProperty() : base(new UpdateCollection<T>())
    {

    }
    public T this[int index] { get => CurrentValue[index]; }

    public int Count => CurrentValue.Count;
}
abstract class UpdateCollectionPropertyBase<T, TCollection> : Property<TCollection>, IEnumerable<T> /* for syntax sugar */ where TCollection : IUpdateReadOnlyCollection<T>
{
    readonly IUpdateCollection<T> currentValue = new UpdateCollection<T>();
    protected IUpdateCollection<T> CurrentValue => currentValue;
    public UpdateCollectionPropertyBase(TCollection collection) : base(collection)
    {
        ValueChanged += UpdateCollectionProperty_ValueChanged;
        ItemsChanged += This_ItemsChanged;
    }


    private void UpdateCollectionProperty_ValueChanged(TCollection oldValue, TCollection newValue)
    {
        newValue.ItemsChanged -= NewValue_ItemsChanged;
        modifying = true;
        currentValue.Clear();
        if (currentValue.Count > 0) throw new InvalidOperationException();
        foreach (var newItem in newValue.AsEnumerable())
            currentValue.Add(newItem);
        modifying = false;
        newValue.ItemsChanged += NewValue_ItemsChanged;
    }

    private void NewValue_ItemsChanged(IEnumerable<IUpdateAction<T>> actions)
    {
        if (!modifying)
        {
            modifying = true;
            foreach (var action in actions)
            {

                switch (action)
                {
                    case ItemsAddedUpdateAction<T> added:
                        for (int i = 0; i < added.Items.Count; i++)
                            currentValue.Insert(added.StartingIndex + i, added.Items[i]);
                        break;
                    case ItemsRemovedUpdateAction<T> removed:
                        for (int i = 0; i < removed.Items.Count; i++)
                            currentValue.RemoveAt(removed.StartingIndex);
                        break;
                    case ItemsMovedUpdateAction<T> moved:
                        currentValue.Move(moved.OldIndex, moved.NewIndex);
                        break;
                    case ItemsReplacedUpdateAction<T> replaced:
                        currentValue[replaced.Index] = replaced.NewItem;
                        break;
                }
            }
            modifying = false;
        }
    }

    private void This_ItemsChanged(IEnumerable<IUpdateAction<T>> actions)
    {
        if (!modifying && this.Value is IUpdateCollection<T> Value)
        {
            modifying = true;
            foreach (var action in actions)
            {

                switch (action)
                {
                    case ItemsAddedUpdateAction<T> added:
                        for (int i = 0; i < added.Items.Count; i++)
                            Value.Insert(added.StartingIndex + i, added.Items[i]);
                        break;
                    case ItemsRemovedUpdateAction<T> removed:
                        for (int i = 0; i < removed.Items.Count; i++)
                            Value.RemoveAt(removed.StartingIndex);
                        break;
                    case ItemsMovedUpdateAction<T> moved:
                        Value.Move(moved.OldIndex, moved.NewIndex);
                        break;
                    case ItemsReplacedUpdateAction<T> replaced:
                        Value[replaced.Index] = replaced.NewItem;
                        break;
                }
            }
            modifying = false;
        }
    }


    bool modifying = false;

    public event UpdateCollectionItemsChanged<T> ItemsChanged
    {
        add
        {
            currentValue.ItemsChanged += value;
        }

        remove
        {
            currentValue.ItemsChanged -= value;
        }
    }

    public IEnumerator<T> GetEnumerator() => currentValue.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}