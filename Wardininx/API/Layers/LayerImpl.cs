using Get.Data.Bindings;
using System.Numerics;
using Wardininx;
using Wardininx.API;
using Wardininx.Core.Layers;

readonly struct LayerImpl(Document d, LayerCore layerCore) : ILayer
{
    // To Do: Rethink about Layer's Canvas Scroll offset and stuff like that
    public int A => ToDo.NotImplemented<int>();
    public IProperty<Vector3> OffsetProperty => new LayerPropertyWrapper(d, layerCore.CanvasScrollOffsetProperty);

    public IProperty<Vector3> ScaleProperty => new LayerPropertyWrapper(d, layerCore.CanvasScaleProperty);

    public Vector3 Offset { get => OffsetProperty.CurrentValue; set => OffsetProperty.CurrentValue = value; }
    public Vector3 Scale { get => OffsetProperty.CurrentValue; set => ScaleProperty.CurrentValue = value; }

    public LayerCore Core => layerCore;

    public IProperty<bool> IsSelectedProperty => layerCore.IsSelectedProperty;

    public bool IsSelected { get => layerCore.IsSelected; set => layerCore.IsSelected = value; }
}
readonly struct LayerPropertyWrapper(Document d, IProperty<Vector3> original) : IProperty<Vector3>
{
    public Vector3 CurrentValue {
        get => original.CurrentValue;
        set
        {
            ToDo.NotImplemented();
            original.CurrentValue = value;
        }
    }

    public event Action RootChanged
    {
        add => original.RootChanged += value;
        remove => original.RootChanged -= value;
    }

    public event ValueChangingHandler<Vector3> ValueChanging
    {
        add => original.ValueChanging += value;

        remove => original.ValueChanging -= value;
    }

    public event ValueChangedHandler<Vector3> ValueChanged
    {
        add => original.ValueChanged += value;

        remove => original.ValueChanged -= value;
    }

    public void Bind(IBinding<Vector3> binding, BindingModes bindingMode)
        => original.Bind(binding, bindingMode);

    public void Bind(IReadOnlyBinding<Vector3> binding, ReadOnlyBindingModes bindingMode)
        => original.Bind(binding, bindingMode);

    void IReadOnlyProperty<Vector3>.BindOneWayToSource(IBinding<Vector3> binding)
        => original.BindOneWayToSource(binding);
}