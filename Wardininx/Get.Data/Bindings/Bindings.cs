using Get.Data.Properties;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Get.Data.Bindings;
public abstract class Binding<TOut>
{
    public abstract TOut Value { get; set; }
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
    public Binding<TNewOut> To<TNewOut>(PropertyDefinitionBase<TOut, TNewOut> pDef)
        => new PathBinding<TOut, TNewOut>(this, pDef);
    public Binding<TNewOut> WithForwardConverter<TNewOut>(ForwardConverter<TOut, TNewOut> converter)
        => new ConvertBinding<TOut, TNewOut>(this, converter, (_, _) => throw new InvalidOperationException("This converter only supports forward binding"));
    public Binding<TNewOut> WithBackwardConverter<TNewOut>(AdvancedBackwardConverter<TOut, TNewOut> converter)
        => new ConvertBinding<TOut, TNewOut>(this, _ => throw new InvalidOperationException("This converter only supports backward binding"), converter);
    public Binding<TNewOut> WithBackwardConverter<TNewOut>(BackwardConverter<TOut, TNewOut> converter)
        => new ConvertBinding<TOut, TNewOut>(this, _ => throw new InvalidOperationException("This converter only supports backward binding"), converter);
    public Binding<TNewOut> WithConverter<TNewOut>(ForwardConverter<TOut, TNewOut> forwardConverter, BackwardConverter<TOut, TNewOut> backwardConverter)
        => new ConvertBinding<TOut, TNewOut>(this, forwardConverter, backwardConverter);
    public Binding<TNewOut> WithConverter<TNewOut>(ForwardConverter<TOut, TNewOut> forwardConverter, AdvancedBackwardConverter<TOut, TNewOut> backwardConverter)
        => new ConvertBinding<TOut, TNewOut>(this, forwardConverter, backwardConverter);
    public Binding<TOut> AllowWritebackWhen(Func<bool> func)
        => WithConverter(x => x, (TOut output, TOut oldInput) => func() ? output : oldInput);
    public Binding<TNewOut> Combine<TIn, TNewOut>(Binding<TIn> binding2, ForwardConverter<TOut, TIn, TNewOut> forwardConverter, AdvancedBackwardConverter<TOut, TIn, TNewOut> backwardConverter)
        => new ConvertBinding<TOut, TIn, TNewOut>(this, binding2, forwardConverter, backwardConverter);
    public Binding<TNewOut> Combine<TIn, TNewOut>(Binding<TIn> binding2, ForwardConverter<TOut, TIn, TNewOut> forwardConverter)
        => Combine(binding2, forwardConverter, (TNewOut _, ref TOut _1, ref TIn _2) => throw new InvalidOperationException("This converter only supports backward binding"));
    public Binding<TNewOut> Combine<TIn2, TIn3, TNewOut>(Binding<TIn2> binding2, Binding<TIn3> binding3, ForwardConverter<TOut, TIn2, TIn3, TNewOut> forwardConverter)
        => Combine(binding2, (a ,b) => (a, b)).Combine(binding3, (x, c) => forwardConverter(x.a, x.b, c));
    public static Binding<TOut> Create(PropertyBase<TOut> pOut)
        => new PropertyBinding<TOut>(pOut);
    public static ValuePathBinding<TIn, TOut> Create<TIn>(TIn root, PropertyDefinitionBase<TIn, TOut> pDef)
        => new(root, pDef);
    public static ValueBinding<TOut> Create(TOut root)
        => new(root);
    public static Binding<TOut> Create<TIn>(Property<TIn> pIn, PropertyDefinitionBase<TIn, TOut> pDef)
        => Binding<TIn>.Create(pIn).To(pDef);
    public static Binding<TOut> Create(ObservableCollection<TOut> collection, Binding<int> index)
        => new ObservableCollectionBinding<TOut>(collection, index);
}
public class ObservableCollectionBinding<TOut>(ObservableCollection<TOut> collection, Binding<int> index) : Binding<TOut?>
{
    TOut value = collection[index.Value];
    public override TOut? Value { get => value; set => collection[index.Value] = value; }
    protected override void RegisterValueChangedEvents()
    {
        collection.CollectionChanged += CollectionChanged;
        index.ValueChanged += InvokeIndexChanged;
    }

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var newValue = Get(index.Value);
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
    TOut? Get(int index) => index >= collection.Count ? default : collection[index];
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
public class ObservableCollectionBindingWithIndex<TOut>(ObservableCollection<TOut> collection, Binding<int> index) : Binding<(TOut? Item, int Index)>
{
    (TOut, int) value;
    public override (TOut? Item, int Index) Value { get => value; set
        {
            if (value.Index != index.Value) throw new InvalidOperationException();
            collection[index.Value] = value.Item;
        }
    }
    protected override void RegisterValueChangedEvents()
    {
        collection.CollectionChanged += CollectionChanged;
        index.ValueChanged += InvokeIndexChanged;
    }

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var newValue = Get(index.Value);
        if (!EqualityComparer<TOut>.Default.Equals(newValue.Item1, value.Item1))
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
    (TOut?, int) Get(int index) => (index >= collection.Count ? default : collection[index], index);
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
public class PropertyBinding<TOut>(PropertyBase<TOut> pOut) : Binding<TOut>
{

    public override TOut Value { get => pOut.Value; set => pOut.Value = value; }
    protected override void RegisterValueChangedEvents()
    {
        pOut.ValueChanged += InvokeValueChanged;
    }
    protected override void UnregisterValueChangedEvents()
    {
        pOut.ValueChanged -= InvokeValueChanged;
    }
    protected override void RegisterValueChangingEvents()
    {
        pOut.ValueChanging += InvokeValueChanging;
    }
    protected override void UnregisterValueChangingEvents()
    {
        pOut.ValueChanging -= InvokeValueChanging;
    }

    protected override void RegisterRootChangedEvents() { }

    protected override void UnregisterRootChangedEvents() { }
}

public class ValuePathBinding<TOwner, TOut>(TOwner initialOwner, PropertyDefinitionBase<TOwner, TOut> pDef) : Binding<TOut>
{
    TOwner owner = initialOwner;
    public TOwner Data
    {
        get => owner;
        set
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
    }
    PropertyBase<TOut> currentProperty = pDef.GetProperty(initialOwner);
    public override TOut Value { get => currentProperty.Value; set => currentProperty.Value = value; }
    protected override void RegisterValueChangedEvents()
    {
        currentProperty.ValueChanged += InvokeValueChanged;
    }
    protected override void UnregisterValueChangedEvents()
    {
        currentProperty.ValueChanged -= InvokeValueChanged;
    }
    protected override void RegisterValueChangingEvents()
    {
        currentProperty.ValueChanging += InvokeValueChanging;
    }
    protected override void UnregisterValueChangingEvents()
    {
        currentProperty.ValueChanging -= InvokeValueChanging;
    }
    protected override void RegisterRootChangedEvents() { }

    protected override void UnregisterRootChangedEvents() { }
}
public class ValueBinding<TOut>(TOut initialOwner) : Binding<TOut>
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
    public override TOut Value { get => Data; set => Data = value; }
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
public delegate TOut ForwardConverter<TIn, TOut>(TIn input);
public delegate TIn BackwardConverter<TIn, TOut>(TOut output);
public delegate TIn AdvancedBackwardConverter<TIn, TOut>(TOut output, TIn oldInput);
class ConvertBinding<TIn, TOut>(Binding<TIn> inBinding, ForwardConverter<TIn, TOut> converter, AdvancedBackwardConverter<TIn, TOut> backwardConverter) : Binding<TOut>
{
    public ConvertBinding(Binding<TIn> inBinding, ForwardConverter<TIn, TOut> converter, BackwardConverter<TIn, TOut> backwardConverter)
        : this(inBinding, converter, (x, _) => backwardConverter(x)) { }
    TIn owner = inBinding.Value;
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
    TOut _value = converter(inBinding.Value);
    public override TOut Value { get => _value; set => inBinding.Value = backwardConverter(value, owner); }
    protected override void RegisterValueChangedEvents()
    {
        inBinding.ValueChanged += InitialOwner_ValueChanged;
        SetData(inBinding.Value);
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
public delegate TOut ForwardConverter<TIn1, TIn2, TOut>(TIn1 input1, TIn2 input2);
public delegate void AdvancedBackwardConverter<TIn1, TIn2, TOut>(TOut output, ref TIn1 input1, ref TIn2 input2);
public delegate TOut ForwardConverter<TIn1, TIn2, TIn3, TOut>(TIn1 input1, TIn2 input2, TIn3 input3);
class ConvertBinding<TIn1, TIn2, TOut>(Binding<TIn1> inBinding1, Binding<TIn2> inBinding2, ForwardConverter<TIn1, TIn2, TOut> converter, AdvancedBackwardConverter<TIn1, TIn2, TOut> backwardConverter) : Binding<TOut>
{
    TIn1 owner1 = inBinding1.Value;
    TIn2 owner2 = inBinding2.Value;
    void SetData(TIn1 value1, TIn2 value2)
    {
        if (EqualityComparer<TIn1>.Default.Equals(owner1, value1) && EqualityComparer<TIn2>.Default.Equals(owner2, value2))
            return;
        UnregisterValueChangingEvents();
        UnregisterValueChangedEvents();
        owner1 = value1;
        owner2 = value2;
        var oldValue = _value;
        var newValue = converter(value1, value2);
        InvokeValueChanging(oldValue, newValue);
        _value = newValue;
        InvokeValueChanged(oldValue, newValue);
        RegisterValueChangingEventsIfNeeded();
        RegisterValueChangedEventsIfNeeded();
    }
    TOut _value = converter(inBinding1.Value, inBinding2.Value);
    public override TOut Value {
        get => _value;
        set
        {
            var val1 = owner1;
            var val2 = owner2;
            backwardConverter(value, ref val1, ref val2);
            inBinding1.Value = val1;
            inBinding2.Value = val2;
        }
    }
    protected override void RegisterValueChangedEvents()
    {
        inBinding1.ValueChanged += InitialOwner_ValueChanged1;
        inBinding2.ValueChanged += InitialOwner_ValueChanged2;
        SetData(inBinding1.Value, inBinding2.Value);
    }

    private void InitialOwner_ValueChanged1(TIn1 oldValue, TIn1 newValue)
    {
        SetData(newValue, owner2);
    }
    private void InitialOwner_ValueChanged2(TIn2 oldValue, TIn2 newValue)
    {
        SetData(owner1, newValue);
    }

    protected override void UnregisterValueChangedEvents()
    {
        inBinding1.ValueChanged -= InitialOwner_ValueChanged1;
        inBinding2.ValueChanged -= InitialOwner_ValueChanged2;
    }
    protected override void RegisterValueChangingEvents()
    {
    }
    protected override void UnregisterValueChangingEvents()
    {
    }
    protected override void RegisterRootChangedEvents()
    {
        inBinding1.RootChanged += InvokeRootChanged;
        inBinding2.RootChanged += InvokeRootChanged;
    }

    protected override void UnregisterRootChangedEvents()
    {
        inBinding1.RootChanged -= InvokeRootChanged;
        inBinding2.RootChanged -= InvokeRootChanged;
    }
}
class PathBinding<TOwner, TOut>(Binding<TOwner> bindingOwner, PropertyDefinitionBase<TOwner, TOut> pDef) : Binding<TOut>
{
    TOwner owner = bindingOwner.Value;
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
    PropertyBase<TOut> currentProperty = pDef.GetProperty(bindingOwner.Value);
    public override TOut Value { get => currentProperty.Value; set => currentProperty.Value = value; }
    protected override void RegisterValueChangedEvents()
    {
        currentProperty.ValueChanged += InvokeValueChanged;
        bindingOwner.ValueChanged += InitialOwner_ValueChanged;
        SetData(bindingOwner.Value);
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
public class RootBinding<TOut>(Binding<TOut> parentBindingInit) : Binding<TOut>
{
    Binding<TOut> parentBinding = parentBindingInit;
    public Binding<TOut> ParentBinding
    {
        get => parentBinding;
        set
        {
            UnregisterValueChangingEvents();
            UnregisterValueChangedEvents();
            parentBinding = value;
            SetData(value.Value);
            RegisterValueChangingEventsIfNeeded();
            RegisterValueChangedEventsIfNeeded();
        }
    }
    TOut owner = parentBindingInit.Value;
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
    public override TOut Value { get => owner; set => throw new InvalidOperationException("You cannot set value of data root"); }
    protected override void RegisterValueChangedEvents()
    {
        ParentBinding.ValueChanged += ParentBinding_ValueChanged;
        SetData(ParentBinding.Value);
    }

    private void ParentBinding_ValueChanged(TOut oldValue, TOut newValue)
    {
        SetData(newValue);
    }

    protected override void UnregisterValueChangedEvents()
    {
        ParentBinding.ValueChanged -= ParentBinding_ValueChanged;
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
public enum BindingModes
{
    /// <summary>
    /// Only change when data root is changed
    /// </summary>
    OneTime,
    OneWay,
    /// <summary>
    /// Same as <see cref="OneWay"/>
    /// </summary>
    OneWayToTarget = OneWay,
    OneWayToSource,
    /// <summary>
    /// Two way binding, update the target value immedietely as the binding is set
    /// </summary>
    TwoWay,
    /// <summary>
    /// Same as <see cref="TwoWay"/>
    /// </summary>
    TwoWayUpdateTargetImmediete = TwoWay,
    /// <summary>
    /// Two way binding, update the source value immedietely as the binding is set
    /// </summary>
    TwoWayUpdateSourceImmediete
}