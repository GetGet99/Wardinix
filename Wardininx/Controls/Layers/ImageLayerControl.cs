#nullable enable
using Windows.Foundation;
using Wardininx.Classes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Wardininx.Core.Layers;

namespace Wardininx.Controls.Layers;
class ImageLayerControl : LayerControl
{
    public ImageLayerCore Core { get; }
    readonly Property<Rect> CanvasBoundsWriter;
    public ImageLayerControl(ImageLayerCore core, Property<Rect> canvasBoundsWriter) : base(core)
    {
        Core = core;
        CanvasBoundsWriter = canvasBoundsWriter;
        Template = App.GUIControlTemplate;
    }
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var core = Core;

        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        var image = new Image();
        gui.Content = image;
        core.ImageProperty.ValueChanged += ImageProperty_ValueChanged;
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
        ImageProperty_ValueChanged(null, core.Image);
        
        var transform = new CompositeTransform();
        RenderTransform = transform;
        // Property Binding
        Core.OffsetProperty.ValueChanged += (_, x) =>
        {
            transform.TranslateX = x.X;
            transform.TranslateY = x.Y;
        };
        transform.TranslateX = Core.Offset.X;
        transform.TranslateY = Core.Offset.Y;
        Core.CanvasScaleProperty.ValueChanged += (_, x) =>
        {
            transform.ScaleX = x.X; transform.ScaleY = x.Y;
        };
        transform.ScaleX = Core.CanvasScale.X;
        transform.ScaleY = Core.CanvasScale.Y;
        Core.IsSelectedProperty.ValueChanged += (_, @new) =>
        {
            IsHitTestVisible = @new;
        };
        IsHitTestVisible = Core.IsSelected;
    }

}