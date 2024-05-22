#nullable enable
using Windows.UI.Xaml;
using Wardininx.Controls.Layers;
using Windows.UI.Xaml.Input;
using Wardininx.API;
namespace Wardininx.Controls.Toolbars;

class WXImageToolbar(WXToolbar parent) : AbstractedUI
{
    public WXToolbar Parent { get; } = parent;
    public Property<ImageLayer?> ImageLayerProperty { get; } = new(null);
    public ImageLayer? ImageLayer { get => ImageLayerProperty.Value; set => ImageLayerProperty.Value = value; }

    protected override UIElement CreateUI() => new WXImageToolbarUI(this);
}

class WXImageToolbarUI : WXControl
{
    public WXImageToolbar Abstracted { get; }
    public WXImageToolbarUI(WXImageToolbar abstracted)
    {
        Abstracted = abstracted;
        abstracted.ImageLayerProperty.ValueChanged += ImageCanvasProperty_ValueChanged;
    }

    private void ImageCanvasProperty_ValueChanged(ImageLayer? oldValue, ImageLayer? newValue)
    {
        //if (oldValue is not null)
        //{
        //    var ele = oldValue.UnsafeGetElement<UIElement>();
        //    ele.ManipulationMode = ManipulationModes.None;
        //    ele.ManipulationDelta -= Ele_ManipulationDelta;
        //}
        //if (newValue is not null)
        //{
        //    var ele = newValue.UnsafeGetElement<UIElement>();
        //    ele.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        //    ele.ManipulationDelta += Ele_ManipulationDelta;
        //}

    }

    //private void Ele_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    //{
    //    var ele = (ImageLayerControl)sender;
    //    ele.Core.Offset += new System.Numerics.Vector3(
    //        (float)e.Delta.Translation.X,
    //        (float)e.Delta.Translation.Y,
    //        0
    //    );
    //}
}