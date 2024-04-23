#nullable enable
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Input.Inking.Core;
using Windows.UI.Xaml;
using Wardininx.Classes;
using Windows.UI.Xaml.Controls;
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
    Image targetImageElement;
    public WXImageCanvas Abstracted { get; }
    readonly Property<Rect> CanvasBoundsWriter;
    public WXImageCanvasUI(WXImageCanvas abstracted, Property<Rect> canvasBoundsWriter)
    {
        Abstracted = abstracted;
        // Property Binding
        Template = App.GUIControlTemplate;
        CanvasBoundsWriter = canvasBoundsWriter;
        abstracted.ImageProperty.ValueChanged += ImageProperty_ValueChanged;
        Abstracted.IsSelectedProperty.ValueChanged += (_, @new) =>
        {
            IsHitTestVisible = @new;
        };
        IsHitTestVisible = Abstracted.IsSelected;
        //var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        //var spriteVisual = compositor.CreateSpriteVisual();
        //CompositionSurfaceBrush surfaceBrush = compositor.CreateSurfaceBrush();
        //surfaceBrush.Surface = LoadedImageSurface.StartLoadFromStream()
    }

    private async void ImageProperty_ValueChanged(WXImage? oldValue, WXImage? newValue)
    {
        if (newValue != null && targetImageElement != null)
        {
            var image = await newValue.GetBitmapImageAsync();
            targetImageElement.Source = image;
        }
    }

    protected override async void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var uc = (UserControl)GetTemplateChild(App.GUIRootName);
        if (uc != null)
        {
            uc.Content = targetImageElement = new Image();
            targetImageElement.SizeChanged += (sender, args) =>
            {
                CanvasBoundsWriter.Value = new(0, 0, targetImageElement.ActualWidth, targetImageElement.ActualHeight);
            };
            CanvasBoundsWriter.Value = new(0, 0, targetImageElement.ActualWidth, targetImageElement.ActualHeight);
            Abstracted.CanvasScrollOffsetProperty.ValueChanged += (_, x) =>
            {
                if (targetImageElement != null) targetImageElement.Translation = x;
            };
            targetImageElement.Translation = Abstracted.CanvasScrollOffset;
            Abstracted.CanvasScaleProperty.ValueChanged += (_, x) =>
            {
                if (targetImageElement != null) targetImageElement.Scale = x;
            };
            targetImageElement.Scale = Abstracted.CanvasScale;
            var newValue = Abstracted.Image;
            if (newValue != null)
            {
                var image = await newValue.GetBitmapImageAsync();
                targetImageElement.Source = image;
            }
        }
    }
}