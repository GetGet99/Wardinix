namespace Get.Data.Bindings;
public class ValueBinding<TOut>(TOut initialOwner) : BindingNotifyBase<TOut>, IBinding<TOut>
{
    TOut owner = initialOwner;
    public TOut Data
    {
        get => owner;
        set
        {
            if (EqualityComparer<TOut>.Default.Equals(owner, value))
                return;
            UnregisterValueChangingEvents();
            UnregisterValueChangedEvents();
            owner = value;
            RegisterValueChangingEventsIfNeeded();
            RegisterValueChangedEventsIfNeeded();
        }
    }
    public TOut CurrentValue { get => Data; set => Data = value; }
    protected override void RegisterValueChangedEvents()
    {

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
