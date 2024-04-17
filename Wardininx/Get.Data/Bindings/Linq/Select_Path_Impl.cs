namespace Get.Data.Bindings.Linq;
abstract class SelectPathBase<TOwner, TOut, TBinding>(IReadOnlyBinding<TOwner> bindingOwner, Func<TOwner, TBinding> pDef) : BindingNotifyBase<TOut> where TBinding : INotifyBinding<TOut>
{
    TOwner owner = bindingOwner.CurrentValue;
    void SetData(TOwner value)
    {
        if (EqualityComparer<TOwner>.Default.Equals(owner, value))
            return;
        UnregisterValueChangingEvents();
        UnregisterValueChangedEvents();
        owner = value;
        currentBinding = pDef(value);
        RegisterValueChangingEventsIfNeeded();
        RegisterValueChangedEventsIfNeeded();
    }
    protected TBinding currentBinding = pDef(bindingOwner.CurrentValue);
    protected override void RegisterValueChangedEvents()
    {
        currentBinding.ValueChanged += InvokeValueChanged;
        bindingOwner.ValueChanged += InitialOwner_ValueChanged;
        SetData(bindingOwner.CurrentValue);
    }

    private void InitialOwner_ValueChanged(TOwner oldValue, TOwner newValue)
    {
        SetData(newValue);
    }

    protected override void UnregisterValueChangedEvents()
    {
        currentBinding.ValueChanged -= InvokeValueChanged;
        bindingOwner.ValueChanged -= InitialOwner_ValueChanged;
    }
    protected override void RegisterValueChangingEvents()
    {
        currentBinding.ValueChanging += InvokeValueChanging;
    }
    protected override void UnregisterValueChangingEvents()
    {
        currentBinding.ValueChanging -= InvokeValueChanging;
    }
    protected override void RegisterRootChangedEvents()
    {
        bindingOwner.RootChanged += InvokeRootChanged;
    }

    protected override void UnregisterRootChangedEvents()
    {
        bindingOwner.RootChanged -= InvokeRootChanged;
    }
}