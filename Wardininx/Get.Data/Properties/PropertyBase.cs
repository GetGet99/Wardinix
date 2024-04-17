using Get.Data.Bindings;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Data;

namespace Get.Data.Properties;

public delegate void ValueChangingHandler<T>(T oldValue, T newValue);
public delegate void ValueChangedHandler<T>(T oldValue, T newValue);
public abstract class PropertyBase<T> : IBinding<T>
{
    public abstract T Value { get; set; }
    T IDataBinding<T>.CurrentValue { get => Value; set => Value = value; }

    T IReadOnlyDataBinding<T>.CurrentValue => Value;

    public abstract event ValueChangingHandler<T>? ValueChanging;
    public abstract event ValueChangedHandler<T>? ValueChanged;
    IReadOnlyBinding<T>? currentBinding;

    // RootChanged event will never be sent
    event Action INotifyBinding<T>.RootChanged
    {
        add { }
        remove { }
    }

    public void Bind(IReadOnlyBinding<T> binding, ReadOnlyBindingModes bindingMode)
    {
        if (currentBinding is not null)
        {
            ValueChanged -= ValueChangedToSourceBinding;
            currentBinding.ValueChanged -= SourceBindingValueChanged;
        }
        currentBinding = binding;
        switch (bindingMode)
        {
            case ReadOnlyBindingModes.OneTime:
                Value = currentBinding.CurrentValue;
                currentBinding.RootChanged += BindingRootChanged;
                break;
            case ReadOnlyBindingModes.OneWayToTarget:
                Value = currentBinding.CurrentValue;
                currentBinding.ValueChanged += SourceBindingValueChanged;
                break;
        }
    }
    public void Bind(IBinding<T> binding, BindingModes bindingMode)
    {
        if (currentBinding is not null)
        {
            ValueChanged -= ValueChangedToSourceBinding;
            currentBinding.ValueChanged -= SourceBindingValueChanged;
        }
        currentBinding = binding;
        switch (bindingMode)
        {
            case BindingModes.OneWayToSource:
                binding.CurrentValue = Value;
                ValueChanged += ValueChangedToSourceBinding;
                break;
            case BindingModes.TwoWayUpdateSourceImmediete:
                binding.CurrentValue = Value;
                ValueChanged += ValueChangedToSourceBinding;
                currentBinding.ValueChanged += SourceBindingValueChanged;
                break;
            case BindingModes.TwoWayUpdateTargetImmediete:
                Value = currentBinding.CurrentValue;
                ValueChanged += ValueChangedToSourceBinding;
                currentBinding.ValueChanged += SourceBindingValueChanged;
                break;
        }
    }

    private void BindingRootChanged()
    {
        if (currentBinding != null)
            Value = currentBinding.CurrentValue;
    }

    void SourceBindingValueChanged(T oldVal, T newVal)
    {
        Value = newVal;
    }
    void ValueChangedToSourceBinding(T oldVal, T newVal)
    {
        if (currentBinding != null && currentBinding is IBinding<T> readWriteBinding)
            readWriteBinding.CurrentValue = newVal;
    }
}
public class ReadOnlyProperty<T>(PropertyBase<T> prop) : PropertyBase<T>
{
    public override T Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => prop.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => throw new InvalidOperationException("This value cannot be set");
    }

    public override event ValueChangingHandler<T> ValueChanging
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        add => prop.ValueChanging += value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        remove => prop.ValueChanging -= value;
    }

    public override event ValueChangedHandler<T> ValueChanged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        add => prop.ValueChanged += value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        remove => prop.ValueChanged -= value;
    }
}
public class Property<T>(T defaultValue) : PropertyBase<T>
{
    T val = defaultValue;
    public override T Value
    {
        get => val;
        set
        {
            if (EqualityComparer<T>.Default.Equals(val, value))
                return;
            var oldValue = val;
            ValueChanging?.Invoke(oldValue, value);
            val = value;
            ValueChanged?.Invoke(oldValue, value);
        }
    }

    public override event ValueChangingHandler<T>? ValueChanging;
    public override event ValueChangedHandler<T>? ValueChanged;
}
public class ExprProperty<T> : PropertyBase<T>
{
    readonly Func<T> _onGetValue;
    readonly Action<T> _onSetValue;
    public ExprProperty(Func<T> onGetValue, Action<T> onSetValue, bool automaticNotifyOnSetValue = true)
    {
        _onGetValue = onGetValue;
        if (automaticNotifyOnSetValue)
            _onSetValue = (value) =>
            {
                onSetValue(value);
            };
        else
            _onSetValue = onSetValue;
    }

    public override T Value { get => _onGetValue(); set => _onSetValue(value); }

    public override event ValueChangingHandler<T>? ValueChanging;
    public override event ValueChangedHandler<T>? ValueChanged;

}
class PropertyWithConverter<TInput, TOutput>(PropertyBase<TInput> originalProperty, Func<TInput, TOutput> forwardConvert, Func<TOutput, TInput> backwardConvert) : PropertyBase<TOutput>
{
    public PropertyWithConverter(PropertyBase<TInput> originalProperty, Func<TInput, TOutput> forwardConvert)
        : this(originalProperty, forwardConvert, x => throw new InvalidOperationException("Backward Converter was not defined")) { }
    readonly PropertyBase<TInput> originalProperty = originalProperty;
    readonly Func<TInput, TOutput> forwardConvert = forwardConvert;
    readonly Func<TOutput, TInput> backwardConvert = backwardConvert;

    public override TOutput Value
    {
        get => forwardConvert(originalProperty.Value);
        set => originalProperty.Value = backwardConvert(value);
    }
    ValueChangingHandler<TOutput>? _ValueChanging;
    public override event ValueChangingHandler<TOutput>? ValueChanging
    {
        add
        {
            if (_ValueChanging is null)
            {
                originalProperty.ValueChanging += ParentValueChangingCallback;
            }
            _ValueChanging += value;
        }
        remove
        {
            _ValueChanging -= value;
            if (_ValueChanging is null)
            {
                originalProperty.ValueChanging -= ParentValueChangingCallback;
            }
        }
    }
    ValueChangedHandler<TOutput>? _ValueChanged;
    public override event ValueChangedHandler<TOutput>? ValueChanged
    {
        add
        {
            if (_ValueChanged is null)
            {
                originalProperty.ValueChanged += ParentValueChangedCallback;
            }
            _ValueChanged += value;
        }
        remove
        {
            _ValueChanged -= value;
            if (_ValueChanged is null)
            {
                originalProperty.ValueChanged -= ParentValueChangedCallback;
            }
        }
    }
    void ParentValueChangingCallback(TInput oldValue, TInput newValue)
        => _ValueChanging?.Invoke(forwardConvert(oldValue), forwardConvert(newValue));
    void ParentValueChangedCallback(TInput oldValue, TInput newValue)
        => _ValueChanged?.Invoke(forwardConvert(oldValue), forwardConvert(newValue));
}