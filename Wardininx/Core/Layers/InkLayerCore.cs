using Wardininx.API;
using Wardininx.Controls.Layers;
using Wardininx.Core.Inking;
using Windows.UI.Xaml;
namespace Wardininx.Core.Layers;
partial class InkLayerCore : LayerCore
{
    public InkControllerCore InkControllerCore { get; } = new()
    {
        InkPresenter =
        {
            InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen, //  | Windows.UI.Core.CoreInputDeviceTypes.Touch 
            IsInputEnabled = true
        }
    };
    public const float RealCanvasSize = 100000; // 1 << 20;
    protected override UIElement CreateUI(Document doc) => new InkLayerControl(this, doc, CanvasBoundsPropertyProtected);
    public override ILayer GetEditingSession(Document owner) => new InkLayer(owner, this);
}