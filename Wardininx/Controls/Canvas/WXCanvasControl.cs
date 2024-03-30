using Get.Data.Properties;
using Get.XAMLTools;
using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Wardininx.Controls.Canvas;
abstract class WXCanvasControl : AbstractedUI {
    public WXCanvasControl()
    {
        CanvasBoundsProperty = new(CanvasBoundsPropertyProtected);
    }
    protected readonly Property<Rect> CanvasBoundsPropertyProtected = new(default);
    public static PropertyDefinition<WXCanvasControl, Rect> CanvasBoundsPropertyDefinition { get; }
        = new(x => x.CanvasBoundsProperty);
    public ReadOnlyProperty<Rect> CanvasBoundsProperty { get; }
    public Rect CanvasBounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasBoundsPropertyProtected.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set => CanvasBoundsPropertyProtected.Value = value;
    }
    public static PropertyDefinition<WXCanvasControl, Vector3> CanvasScalePropertyDefinition { get; }
        = new(x => x.CanvasScaleProperty);
    public Property<Vector3> CanvasScaleProperty { get; } = new(Vector3.One);
    public Vector3 CanvasScale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasScaleProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CanvasScaleProperty.Value = value;
    }
    public static PropertyDefinition<WXCanvasControl, Vector3> CanvasScrollOffsetPropertyDefinition { get; }
        = new(x => x.CanvasScrollOffsetProperty);
    public Property<Vector3> CanvasScrollOffsetProperty { get; } = new(default);
    public Vector3 CanvasScrollOffset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasScrollOffsetProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CanvasScrollOffsetProperty.Value = value;
    }
}
abstract partial class WXCanvasControlUI : WXControl
{
}