using System.Numerics;
using Wardininx.Core.Layers;

namespace Wardininx.API;

public interface ILayer : IEditingSession<LayerCore>
{
    IProperty<Vector3> OffsetProperty { get; }
    IProperty<Vector3> ScaleProperty { get; }
    IProperty<bool> IsSelectedProperty { get; }
    Vector3 Offset { get; set; }
    Vector3 Scale { get; set; }
    bool IsSelected { get; set; }
}