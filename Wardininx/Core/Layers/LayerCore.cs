using System.Numerics;
using System.Runtime.CompilerServices;
using Wardininx.API;
using Wardininx.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Wardininx.Core.Layers;

public abstract class LayerCore : AbstractedUIDocument
{
    public LayerCore()
    {
        CanvasBoundsProperty = new(CanvasBoundsPropertyProtected);
        IsSelected = false;
    }
    protected readonly Property<Rect> CanvasBoundsPropertyProtected = new(default);
    public static IReadOnlyPropertyDefinition<LayerCore, Rect> CanvasBoundsPropertyDefinition { get; }
        = new ReadOnlyPropertyDefinition<LayerCore, Rect>(x => x.CanvasBoundsProperty);
    public ReadOnlyProperty<Rect> CanvasBoundsProperty { get; }
    public Rect CanvasBounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasBoundsPropertyProtected.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set => CanvasBoundsPropertyProtected.Value = value;
    }
    public static PropertyDefinition<LayerCore, Vector3> CanvasScalePropertyDefinition { get; }
        = new(x => x.CanvasScaleProperty);
    public Property<Vector3> CanvasScaleProperty { get; } = new(Vector3.One);
    public Vector3 CanvasScale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasScaleProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CanvasScaleProperty.Value = value;
    }
    public static PropertyDefinition<LayerCore, Vector3> CanvasScrollOffsetPropertyDefinition { get; }
        = new(x => x.CanvasScrollOffsetProperty);
    public Property<Vector3> CanvasScrollOffsetProperty { get; } = new(default);
    public Vector3 CanvasScrollOffset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasScrollOffsetProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CanvasScrollOffsetProperty.Value = value;
    }
    public Property<bool> IsSelectedProperty { get; } = new(default);
    public bool IsSelected { get => IsSelectedProperty.Value; set => IsSelectedProperty.Value = value; }

    public abstract ILayer GetEditingSession(Document owner);
}