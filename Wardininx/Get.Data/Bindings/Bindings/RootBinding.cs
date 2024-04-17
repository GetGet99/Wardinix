namespace Get.Data.Bindings;
public class RootBinding<TOut> : BindingNotifyBase<TOut>, IReadOnlyBinding<TOut>
{
    public TOut CurrentValue => owner;
    public RootBinding(IReadOnlyBinding<TOut> parentBindingInit)
    {
        owner = parentBindingInit.CurrentValue;
        // set to property to run the property registration code
        ParentBinding = parentBinding = parentBindingInit;
        SetData(ParentBinding.CurrentValue);
    }
    IReadOnlyBinding<TOut> parentBinding;
    public IReadOnlyBinding<TOut> ParentBinding
    {
        get => parentBinding;
        set
        {
            UnregisterValueChangingEvents();
            UnregisterValueChangedEvents();
            parentBinding.ValueChanged -= ParentBinding_ValueChanged;
            parentBinding = value;
            SetData(value.CurrentValue);
            value.ValueChanged += ParentBinding_ValueChanged;
            RegisterValueChangingEventsIfNeeded();
            RegisterValueChangedEventsIfNeeded();
        }
    }
    TOut owner;
    void SetData(TOut value)
    {
        if (EqualityComparer<TOut>.Default.Equals(owner, value))
            return;
        UnregisterValueChangingEvents();
        UnregisterValueChangedEvents();
        var oldValue = owner;
        InvokeValueChanging(oldValue, value);
        owner = value;
        InvokeRootChanged();
        InvokeValueChanged(oldValue, value);
        RegisterValueChangingEventsIfNeeded();
        RegisterValueChangedEventsIfNeeded();
    }
    protected override void RegisterValueChangedEvents()
    {
    }

    private void ParentBinding_ValueChanged(TOut oldValue, TOut newValue)
    {
        SetData(newValue);
    }

    protected override void UnregisterValueChangedEvents()
    {
    }
    protected override void RegisterValueChangingEvents()
    {

    }
    protected override void UnregisterValueChangingEvents()
    {

    }
    protected override void RegisterRootChangedEvents() { }

    protected override void UnregisterRootChangedEvents() { }
}