using System.Numerics;
using Wardininx.Classes;
using Wardininx.Core.Layers;

namespace Wardininx.API;
readonly struct ImageLayer(Document document, ImageLayerCore imageLayerCore) : ILayer, IEditingSession<ImageLayerCore>
{
    public WXImage Image
    {
        get => imageLayerCore.Image;
        set => ToDo.NotImplemented();
    }

    public IProperty<Vector3> OffsetProperty => new LayerImpl(document, imageLayerCore).OffsetProperty;

    public IProperty<Vector3> ScaleProperty => new LayerImpl(document, imageLayerCore).ScaleProperty;

    public Vector3 Offset { get => OffsetProperty.CurrentValue; set => OffsetProperty.CurrentValue = value; }
    public Vector3 Scale { get => ScaleProperty.CurrentValue; set => ScaleProperty.CurrentValue = value; }

    public ImageLayerCore Core => imageLayerCore;

    public IProperty<bool> IsSelectedProperty => new LayerImpl(document, imageLayerCore).IsSelectedProperty;

    public bool IsSelected { get => IsSelectedProperty.CurrentValue; set => IsSelectedProperty.CurrentValue = value; }

    LayerCore IEditingSession<LayerCore>.Core => imageLayerCore;
}