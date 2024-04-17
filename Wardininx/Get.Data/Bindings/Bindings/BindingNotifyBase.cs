using Get.Data.Bindings;
using Get.Data.Properties;

public abstract class BindingNotifyBase<TOut> : INotifyBinding<TOut>
{
    Action? _RootChanged;
    ValueChangingHandler<TOut>? _ValueChanging;
    ValueChangedHandler<TOut>? _ValueChanged;
    public event Action RootChanged
    {
        add
        {
            if (_RootChanged is null)
            {
                RegisterRootChangedEvents();
            }
            _RootChanged += value;
        }
        remove
        {
            _RootChanged -= value;

            if (_RootChanged is null)
            {
                UnregisterRootChangedEvents();
            }
        }
    }
    public event ValueChangingHandler<TOut>? ValueChanging
    {
        add
        {
            if (_ValueChanging is null)
            {
                RegisterValueChangingEvents();
            }
            _ValueChanging += value;
        }
        remove
        {
            _ValueChanging -= value;

            if (_ValueChanging is null)
            {
                UnregisterValueChangingEvents();
            }
        }
    }
    public event ValueChangedHandler<TOut>? ValueChanged
    {
        add
        {
            if (_ValueChanged is null)
            {
                RegisterValueChangedEvents();
            }
            _ValueChanged += value;
        }
        remove
        {
            _ValueChanged -= value;

            if (_ValueChanged is null)
            {
                UnregisterValueChangedEvents();
            }
        }
    }
    protected void InvokeValueChanging(TOut oldValue, TOut newValue)
        => _ValueChanging?.Invoke(oldValue, newValue);
    protected void InvokeValueChanged(TOut oldValue, TOut newValue)
        => _ValueChanged?.Invoke(oldValue, newValue);
    protected void InvokeRootChanged()
        => _RootChanged?.Invoke();
    protected void RegisterValueChangingEventsIfNeeded()
    {
        if (_ValueChanging is not null)
            RegisterValueChangingEvents();
    }
    protected void RegisterValueChangedEventsIfNeeded()
    {
        if (_ValueChanged is not null)
            RegisterValueChangedEvents();
    }
    protected abstract void RegisterRootChangedEvents();
    protected abstract void RegisterValueChangingEvents();
    protected abstract void RegisterValueChangedEvents();
    protected abstract void UnregisterRootChangedEvents();
    protected abstract void UnregisterValueChangingEvents();
    protected abstract void UnregisterValueChangedEvents();
}