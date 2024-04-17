using Get.Data.Collections.Update;

namespace Get.Data.Bindings.Linq;
abstract class ElementAtBase<TOut>(IUpdateReadOnlyCollection<TOut> collection, IReadOnlyBinding<int> index) : BindingNotifyBase<TOut?>
{
    protected TOut? value = index.CurrentValue >= 0 ? collection[index.CurrentValue] : default;
    //public override TOut? CurrentValue { get => value; set => throw new InvalidOperationException("Cannot go backwards"); }
    protected override void RegisterValueChangedEvents()
    {
        collection.ItemsChanged += Collection_ItemsChanged;
        index.ValueChanged += InvokeIndexChanged;
    }

    private void Collection_ItemsChanged(IEnumerable<IUpdateAction<TOut>> actions)
    {
        UpdateValue();
    }
    void UpdateValue()
    {
        var newValue = Get(index.CurrentValue);
        if (!EqualityComparer<TOut>.Default.Equals(newValue, value))
        {
            var oldValue = value;
            InvokeValueChanging(oldValue, newValue);
            value = newValue;
            InvokeValueChanged(oldValue, newValue);
        }
    }

    protected override void UnregisterValueChangedEvents()
    {
        index.ValueChanged -= InvokeIndexChanged;
    }
    protected override void RegisterValueChangingEvents()
    {
        index.ValueChanging += InvokeIndexChanging;
    }
    protected override void UnregisterValueChangingEvents()
    {
        index.ValueChanging -= InvokeIndexChanging;
    }
    TOut? Get(int index) => index >= collection.Count || index < 0 ? default : collection[index];
    protected void InvokeIndexChanging(int oldIndex, int newIndex) => InvokeValueChanging(value, Get(newIndex));
    protected void InvokeIndexChanged(int oldIndex, int newIndex)
    {
        var oldValue = value;
        value = Get(newIndex);
        InvokeValueChanged(oldValue, value);
    }

    protected override void RegisterRootChangedEvents() { }

    protected override void UnregisterRootChangedEvents() { }

}