using System.Numerics;
using Wardininx.Classes;
using Wardininx.Controls.Canvas;

namespace Wardininx.API;
readonly struct InkLayer(Document document, InkLayerCore inkLayerCore) : ILayer, IEditingSession<InkLayerCore>
{
    public InkController InkController => inkLayerCore.InkController;

    public IProperty<Vector3> OffsetProperty => new LayerImpl(document, inkLayerCore).OffsetProperty;

    public IProperty<Vector3> ScaleProperty => new LayerImpl(document, inkLayerCore).ScaleProperty;

    public Vector3 Offset { get => OffsetProperty.CurrentValue; set => OffsetProperty.CurrentValue = value; }
    public Vector3 Scale { get => ScaleProperty.CurrentValue; set => ScaleProperty.CurrentValue = value; }

    public InkLayerCore Core => inkLayerCore;
    LayerCore IEditingSession<LayerCore>.Core => inkLayerCore;
}