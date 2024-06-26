using System.Numerics;
using System.Runtime.CompilerServices;
using Wardininx.Core;

namespace Wardininx.API;
partial class Document
{
    readonly DocumentCore Core;
    public Document(DocumentCore documentCore)
    {
        Layers = new(this, (documentCore as IRawDocumentEdit).Layers);
        Core = documentCore;
    }

    public int SelectedIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => SelectedIndexProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SelectedIndexProperty.Value = value;
    }
    public Vector3 CanvasViewOffset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasViewOffsetProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CanvasViewOffsetProperty.Value = value;
    }
    public float CanvasViewScale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasViewScaleProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CanvasViewScaleProperty.Value = value;
    }
    public bool IsActiveView
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => IsActiveViewProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => IsActiveViewProperty.Value = value;
    }
    public PropertyDefinition<Document, int> SelectedIndexPropertyDefinition { get; } = new(x => x.SelectedIndexProperty);
    public PropertyDefinition<Document, Vector3> CanvasViewOffsetPropertyDefinition { get; } = new(x => x.CanvasViewOffsetProperty);
    public PropertyDefinition<Document, float> CanvasViewScalePropertyDefinition { get; } = new(x => x.CanvasViewScaleProperty);
    public PropertyDefinition<Document, bool> IsActiveViewPropertyDefinition { get; } = new(x => x.IsActiveViewProperty);
}