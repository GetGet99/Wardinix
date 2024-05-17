#nullable enable
using System.Numerics;
using Wardininx.Core;
using Wardininx.UndoRedos;
using Windows.Data.Json;

namespace Wardininx.API;

public partial class Document : IEditingSession<DocumentCore>
{
    
    public UndoManager UndoManager { get; } = new();
    public LayersCollection Layers { get; }
    public Property<int> SelectedIndexProperty { get; } = new(0);
    public Property<Vector3> CanvasViewOffsetProperty { get; } = new(default);
    public Property<float> CanvasViewScaleProperty { get; } = new(default);

    DocumentCore IEditingSession<DocumentCore>.Core => Core;

    public static void Create(JsonObject json) {
        
    }
}