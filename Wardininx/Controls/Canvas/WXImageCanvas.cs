#nullable enable
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Wardininx.Classes;
using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Wardininx.Controls.Canvas;

class WXImageCanvas : WXCanvasControl
{
    public Property<WXImage?> ImageProperty { get; } = new(null);
    public WXImage? Image { get => ImageProperty.Value; set => ImageProperty.Value = value; }
    public Property<Vector3> OffsetProperty { get; } = new(default);
    public Vector3 Offset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => OffsetProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => OffsetProperty.Value = value;
    }
    protected override UIElement CreateUI() => new WXImageCanvasUI(this, CanvasBoundsPropertyProtected);
}
class WXImageCanvasUI : WXCanvasControlUI
{
    public WXImageCanvas Abstracted { get; }
    public WXImageCanvasUI(WXImageCanvas abstracted, Property<Rect> canvasBoundsWriter)
    {
        Abstracted = abstracted;
        RealRootVisual = ElementCompositionPreview.GetElementVisual(this);
        Compositor = RealRootVisual.Compositor;
        RootVisual = Compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(this, RootVisual);
        RootVisual.Clip = Compositor.CreateInsetClip(0, 0, 0, 0);
        RootVisual.RelativeSizeAdjustment = Vector2.One;
        UserOffsetVisual = Compositor.CreateContainerVisual();
        UserOffsetVisual.Size = new(WXInkCanvas.RealCanvasSize, WXInkCanvas.RealCanvasSize);
        UserOffsetVisual.Offset = default;
        UserOffsetVisual.Comment = "UserOffsetVisual";
        RootVisual.Children.InsertAtTop(UserOffsetVisual);
        ImageVisual = Compositor.CreateSpriteVisual();
        ImageVisual.Comment = "ImageVisual";
        UserOffsetVisual.Children.InsertAtTop(ImageVisual);
        CompositionSurfaceBrush surfaceBrush = Compositor.CreateSurfaceBrush();
        ImageVisual.Brush = surfaceBrush;
        abstracted.ImageProperty.ValueChanged += ImageProperty_ValueChanged;
        async void ImageProperty_ValueChanged(WXImage? oldValue, WXImage? newValue)
        {
            if (newValue is not null)
            {
                (surfaceBrush.Surface as LoadedImageSurface)?.Dispose();
                LoadedImageSurface l;
                surfaceBrush.Surface = l = await newValue.GetImageSurfaceAsync();
                ImageVisual.Size = new((float)l.DecodedSize.Width, (float)l.DecodedSize.Height);
                canvasBoundsWriter.Value = new(default, l.DecodedSize);
            }
        };
        // update
        ImageProperty_ValueChanged(null, abstracted.Image);
        // Property Binding
        Abstracted.CanvasScrollOffsetProperty.ValueChanged += (_, x) => UserOffsetVisual.Offset = x;
        Abstracted.CanvasScaleProperty.ValueChanged += (_, x) => UserOffsetVisual.Scale = x;
        Abstracted.IsSelectedProperty.ValueChanged += (_, @new) =>
        {
            RootVisual.IsHitTestVisible = @new;
        };
        RootVisual.IsHitTestVisible = Abstracted.IsSelected;
        Abstracted.IsSelectedProperty.ValueChanged += (_, @new) =>
        {
            IsHitTestVisible = @new;
        };
        IsHitTestVisible = Abstracted.IsSelected;
    }


    readonly Compositor Compositor;
    readonly ContainerVisual UserOffsetVisual;
    readonly SpriteVisual ImageVisual;
    readonly ContainerVisual RootVisual;
    readonly Visual RealRootVisual;
}