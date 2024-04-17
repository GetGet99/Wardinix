using Get.Data.Collections.Update;
using Get.Data.Properties;

namespace Get.Data.Bindings.Linq;
class SelectPropertyPathBase<TOwner, TOut>(IReadOnlyBinding<TOwner> bindingOwner, IPropertyDefinition<TOwner, TOut> pDef) : BindingNotifyBase<TOut>
{
    TOwner owner = bindingOwner.CurrentValue;
    void SetData(TOwner value)
    {
        if (EqualityComparer<TOwner>.Default.Equals(owner, value))
            return;
        UnregisterValueChangingEvents();
        UnregisterValueChangedEvents();
        owner = value;
        currentProperty = pDef.GetProperty(value);
        RegisterValueChangingEventsIfNeeded();
        RegisterValueChangedEventsIfNeeded();
    }
    protected PropertyBase<TOut> currentProperty = pDef.GetProperty(bindingOwner.CurrentValue);
    protected override void RegisterValueChangedEvents()
    {
        currentProperty.ValueChanged += InvokeValueChanged;
        bindingOwner.ValueChanged += InitialOwner_ValueChanged;
        SetData(bindingOwner.CurrentValue);
    }

    private void InitialOwner_ValueChanged(TOwner oldValue, TOwner newValue)
    {
        SetData(newValue);
    }

    protected override void UnregisterValueChangedEvents()
    {
        currentProperty.ValueChanged -= InvokeValueChanged;
        bindingOwner.ValueChanged -= InitialOwner_ValueChanged;
    }
    protected override void RegisterValueChangingEvents()
    {
        currentProperty.ValueChanging += InvokeValueChanging;
    }
    protected override void UnregisterValueChangingEvents()
    {
        currentProperty.ValueChanging -= InvokeValueChanging;
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