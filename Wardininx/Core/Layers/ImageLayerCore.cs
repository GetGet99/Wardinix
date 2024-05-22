using System.Numerics;
using System.Runtime.CompilerServices;
using Wardininx.API;
using Wardininx.Classes;
using Wardininx.Controls.Layers;
using Windows.UI.Xaml;

namespace Wardininx.Core.Layers;
class ImageLayerCore : LayerCore
{
    public Property<WXImage?> ImageProperty { get; } = new(null);
    public WXImage? Image { get => ImageProperty.Value; set => ImageProperty.Value = value; }
    public Property<Vector3> OffsetProperty { get; } = new(default);
    public Vector3 Offset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => OffsetProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => OffsetProperty.Value = value;
    }
    protected override UIElement CreateUI(Document doc) => new ImageLayerControl(this, CanvasBoundsPropertyProtected);
    public override ILayer GetEditingSession(Document owner) => new ImageLayer(owner, this);
}