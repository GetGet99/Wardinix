using Get.Data.Collections.Update;

namespace Get.Data.Bindings.Linq;
abstract class SelectBase<TIn, TOut>(IReadOnlyBinding<TIn> inBinding, ForwardConverter<TIn, TOut> converter)
    : BindingNotifyBase<TOut>
{
    protected TIn owner = inBinding.CurrentValue;
    void SetData(TIn value)
    {
        if (EqualityComparer<TIn>.Default.Equals(owner, value))
            return;
        UnregisterValueChangingEvents();
        UnregisterValueChangedEvents();
        owner = value;
        var oldValue = _value;
        var newValue = converter(value);
        InvokeValueChanging(oldValue, newValue);
        _value = newValue;
        InvokeValueChanged(oldValue, newValue);
        RegisterValueChangingEventsIfNeeded();
        RegisterValueChangedEventsIfNeeded();
    }
    protected TOut _value = converter(inBinding.CurrentValue);
    protected override void RegisterValueChangedEvents()
    {
        inBinding.ValueChanged += InitialOwner_ValueChanged;
        SetData(inBinding.CurrentValue);
    }

    private void InitialOwner_ValueChanged(TIn oldValue, TIn newValue)
    {
        SetData(newValue);
    }

    protected override void UnregisterValueChangedEvents()
    {
        inBinding.ValueChanged -= InitialOwner_ValueChanged;
    }
    protected override void RegisterValueChangingEvents()
    {
    }
    protected override void UnregisterValueChangingEvents()
    {
    }
    protected override void RegisterRootChangedEvents()
    {
        inBinding.RootChanged += InvokeRootChanged;
    }

    protected override void UnregisterRootChangedEvents()
    {
        inBinding.RootChanged -= InvokeRootChanged;
    }
}