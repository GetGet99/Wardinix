#nullable enable
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Wardininx.Classes;
using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Wardininx.Controls.Canvas;

class ImageLayerCore : LayerCore
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
    public ImageLayerCore Abstracted { get; }
    Property<Rect> CanvasBoundsWriter;
    public WXImageCanvasUI(ImageLayerCore abstracted, Property<Rect> canvasBoundsWriter)
    {
        Abstracted = abstracted;
        CanvasBoundsWriter = canvasBoundsWriter;
        Template = App.GUIControlTemplate;
    }
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var abstracted = Abstracted;

        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        var image = new Image();
        gui.Content = image;
        abstracted.ImageProperty.ValueChanged += ImageProperty_ValueChanged;
        async void ImageProperty_ValueChanged(WXImage? oldValue, WXImage? newValue)
        {
            if (newValue is not null)
            {
                var img = await newValue.GetBitmapImageAsync();
                image.Source = img;
                CanvasBoundsWriter.Value = new(default, new Size(img.DecodePixelWidth, img.DecodePixelHeight));
            }
        };
        // update
        ImageProperty_ValueChanged(null, abstracted.Image);
        
        var transform = new CompositeTransform();
        RenderTransform = transform;
        // Property Binding
        Abstracted.OffsetProperty.ValueChanged += (_, x) =>
        {
            transform.TranslateX = x.X;
            transform.TranslateY = x.Y;
        };
        transform.TranslateX = Abstracted.Offset.X;
        transform.TranslateY = Abstracted.Offset.Y;
        Abstracted.CanvasScaleProperty.ValueChanged += (_, x) =>
        {
            transform.ScaleX = x.X; transform.ScaleY = x.Y;
        };
        transform.ScaleX = Abstracted.CanvasScale.X;
        transform.ScaleY = Abstracted.CanvasScale.Y;
        Abstracted.IsSelectedProperty.ValueChanged += (_, @new) =>
        {
            IsHitTestVisible = @new;
        };
        IsHitTestVisible = Abstracted.IsSelected;
    }

}