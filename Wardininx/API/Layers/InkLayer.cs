using System.Numerics;
using Wardininx.API.Inking;
using Wardininx.Core.Layers;

namespace Wardininx.API;
readonly struct InkLayer(Document document, InkLayerCore inkLayerCore) : ILayer, IEditingSession<InkLayerCore>
{
    public InkController InkController => new(document, inkLayerCore.InkControllerCore);

    public IProperty<Vector3> OffsetProperty => new LayerImpl(document, inkLayerCore).OffsetProperty;

    public IProperty<Vector3> ScaleProperty => new LayerImpl(document, inkLayerCore).ScaleProperty;

    public Vector3 Offset { get => OffsetProperty.CurrentValue; set => OffsetProperty.CurrentValue = value; }
    public Vector3 Scale { get => ScaleProperty.CurrentValue; set => ScaleProperty.CurrentValue = value; }

    public InkLayerCore Core => inkLayerCore;
    LayerCore IEditingSession<LayerCore>.Core => inkLayerCore;
    public IProperty<bool> IsSelectedProperty => new LayerImpl(document, inkLayerCore).IsSelectedProperty;

    public bool IsSelected { get => IsSelectedProperty.CurrentValue; set => IsSelectedProperty.CurrentValue = value; }
}