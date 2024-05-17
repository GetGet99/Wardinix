using System.Numerics;
using Wardininx.Controls.Canvas;

namespace Wardininx.API;

public interface ILayer : IEditingSession<LayerCore>
{
    IProperty<Vector3> OffsetProperty { get; }
    IProperty<Vector3> ScaleProperty { get; }
    Vector3 Offset { get; set; }
    Vector3 Scale { get; set; }
}