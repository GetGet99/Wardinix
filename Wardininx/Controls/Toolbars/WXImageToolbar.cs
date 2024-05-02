#nullable enable
using Get.Data.Helpers;
using Get.Data.Properties;
using Get.Data.Bindings;
using Get.Data.XACL;
using Get.Data.Collections.Linq;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Wardininx.Classes;
using Get.Symbols;
using Get.Data.Collections;
using Windows.UI.ViewManagement;
using Get.Data.Collections.Update;
using Get.Data.Bindings.Linq;
using Wardininx.Controls.Canvas;
using Windows.UI.Xaml.Input;
namespace Wardininx.Controls.Toolbars;

class WXImageToolbar(WXToolbar parent) : AbstractedUI
{
    public WXToolbar Parent { get; } = parent;
    public Property<WXImageCanvas?> ImageCanvasProperty { get; } = new(null);
    public WXImageCanvas? ImageCanvas { get => ImageCanvasProperty.Value; set => ImageCanvasProperty.Value = value; }

    protected override UIElement CreateUI() => new WXImageToolbarUI(this);
}

class WXImageToolbarUI : WXControl
{
    public WXImageToolbar Abstracted { get; }
    public WXImageToolbarUI(WXImageToolbar abstracted)
    {
        Abstracted = abstracted;
        abstracted.ImageCanvasProperty.ValueChanged += ImageCanvasProperty_ValueChanged;
    }

    private void ImageCanvasProperty_ValueChanged(WXImageCanvas? oldValue, WXImageCanvas? newValue)
    {
        if (oldValue is not null)
        {
            var ele = oldValue.UnsafeGetElement<UIElement>();
            ele.ManipulationMode = ManipulationModes.None;
            ele.ManipulationDelta -= Ele_ManipulationDelta;
        }
        if (newValue is not null)
        {
            var ele = newValue.UnsafeGetElement<UIElement>();
            ele.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            ele.ManipulationDelta += Ele_ManipulationDelta;
        }

    }

    private void Ele_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        var ele = (WXImageCanvasUI)sender;
        ele.Abstracted.Offset += new System.Numerics.Vector3(
            (float)e.Delta.Translation.X,
            (float)e.Delta.Translation.Y,
            0
        );
    }
}