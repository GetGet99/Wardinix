using Get.Data.Collections;
using Wardininx.API;
using Wardininx.Controls.Layers;
using Windows.UI.Xaml;
namespace Wardininx.Core.Layers;
class GroupLayerCore : LayerCore
{
    public TwoWayUpdateCollectionProperty<LayerCore> Children { get; } = new();
    protected override UIElement CreateUI(Document doc) => new GroupLayerControl(this, doc, CanvasBoundsPropertyProtected);
    public override ILayer GetEditingSession(Document owner) => ToDo.NotImplemented<ILayer>();
}