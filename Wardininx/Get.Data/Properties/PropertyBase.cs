using Get.Data.Bindings;
using System.Runtime.CompilerServices;

namespace Get.Data.Properties;

public delegate void ValueChangingHandler<T>(T oldValue, T newValue);
public delegate void ValueChangedHandler<T>(T oldValue, T newValue);
public abstract class PropertyBase<T>
{
    public abstract T Value { get; set; }
    public abstract event ValueChangingHandler<T>? ValueChanging;
    public abstract event ValueChangedHandler<T>? ValueChanged;
    Binding<T>? currentBinding;
    public Binding<T> ToBinding() => Binding<T>.Create(this);
    public void Bind(Binding<T> binding, BindingModes bindingMode)
    {
        if (currentBinding is not null)
        {
            ValueChanged -= ValueChangedToSourceBinding;
            currentBinding.ValueChanged -= SourceBindingValueChanged;
        }
        currentBinding = binding;
        switch (bindingMode)
        {
            case BindingModes.OneTime:
                Value = currentBinding.Value;
                currentBinding.RootChanged += BindingRootChanged;
                break;
            case BindingModes.OneWayToTarget:
                Value = currentBinding.Value;
                currentBinding.ValueChanged += SourceBindingValueChanged;
                break;
            case BindingModes.OneWayToSource:
                currentBinding.Value = Value;
                ValueChanged += ValueChangedToSourceBinding;
                break;
            case BindingModes.TwoWayUpdateSourceImmediete:
                currentBinding.Value = Value;
                ValueChanged += ValueChangedToSourceBinding;
                currentBinding.ValueChanged += SourceBindingValueChanged;
                break;
            case BindingModes.TwoWayUpdateTargetImmediete:
                Value = currentBinding.Value;
                ValueChanged += ValueChangedToSourceBinding;
                currentBinding.ValueChanged += SourceBindingValueChanged;
                break;
        }
    }

    private void BindingRootChanged()
    {
        if (currentBinding != null)
            Value = currentBinding.Value;
    }

    void SourceBindingValueChanged(T oldVal, T newVal)
    {
        Value = newVal;
    }
    void ValueChangedToSourceBinding(T oldVal, T newVal)
    {
        if (currentBinding != null)
            currentBinding.Value = newVal;
    }
    public static implicit operator Binding<T>(PropertyBase<T> prop) => prop.ToBinding();
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