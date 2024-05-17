using Get.Data.Helpers;
using Get.Data.Properties;
using System.Numerics;
using Wardininx.API;
using Wardininx.API.Inking;
using Wardininx.Core.Inking;
using Wardininx.UndoRedos;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Wardininx.Controls.Canvas;

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
    protected override UIElement CreateUI() => new WXInkCanvasUI(this, CanvasBoundsPropertyProtected);
}
partial class WXInkCanvasUI : WXCanvasControlUI
{
    public InkLayerCore Abstracted { get; }
    readonly Property<Rect> CanvasBoundsWriter;
    public WXInkCanvasUI(InkLayerCore abstracted, Property<Rect> canvasBoundsWriter)
    {
        Background = new SolidColorBrush(Colors.Transparent);
        Abstracted = abstracted;
        CanvasBoundsWriter = canvasBoundsWriter;
        // Setup
        RealRootVisual = ElementCompositionPreview.GetElementVisual(this);
        Compositor = RealRootVisual.Compositor;
        RootVisual = Compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(this, RootVisual);
        //RootVisual.Clip = Compositor.CreateInsetClip(0, 0, 0, 0);
        RootVisual.RelativeSizeAdjustment = Vector2.One;
        UserOffsetVisual = Compositor.CreateContainerVisual();
        UserOffsetVisual.Size = new(InkLayerCore.RealCanvasSize, InkLayerCore.RealCanvasSize);
        UserOffsetVisual.Offset = default;
        RootVisual.Children.InsertAtTop(UserOffsetVisual);
        InternalOffsetVisual = Compositor.CreateContainerVisual();
        InternalOffsetVisual.Size = new(InkLayerCore.RealCanvasSize, InkLayerCore.RealCanvasSize);
        InternalOffsetVisual.Offset = new(-InkLayerCore.RealCanvasSize / 2, -InkLayerCore.RealCanvasSize / 2, 0);
        UserOffsetVisual.Children.InsertAtTop(InternalOffsetVisual);
        InfiniteVisual = Compositor.CreateContainerVisual();
        InternalOffsetVisual.Children.InsertAtTop(InfiniteVisual);
        InfiniteVisual.Size = new(InkLayerCore.RealCanvasSize, InkLayerCore.RealCanvasSize);
        InfiniteVisual.Offset = default;
        CoreInkPresenterHost.RootVisual = InfiniteVisual;
        var InkPresenter = (Abstracted.InkController as IEditingSession<InkControllerCore>).Core.InkPresenter;
        InkPresenter.StrokesCollected += (_, _) => UpdateBounds();
        InkPresenter.StrokesErased += (_, _) => UpdateBounds();
        void UpdateBounds() {
            var rect = InkPresenter.StrokeContainer.BoundingRect;
            rect.X -= InkLayerCore.RealCanvasSize / 2;
            rect.Y -= InkLayerCore.RealCanvasSize / 2;
            CanvasBoundsWriter.Value = rect;
        }
        // Property Binding
        InfiniteVisual.Offset = Abstracted.CanvasScrollOffset;
        Abstracted.CanvasScrollOffsetProperty.ValueChanged += (_, x) => UserOffsetVisual.Offset = x;
        InfiniteVisual.Offset = Abstracted.CanvasScrollOffset;
        Abstracted.CanvasScaleProperty.ValueChanged += (_, x) => UserOffsetVisual.Scale = x;
        Abstracted.IsSelectedProperty.ValueChanged += (_, @new) =>
        {
            RootVisual.IsHitTestVisible = @new;
        };
        RootVisual.IsHitTestVisible = Abstracted.IsSelected;
        Loaded += WXInkCanvasUI_Loaded;
        Unloaded += WXInkCanvasUI_Unloaded;
    }

    private void WXInkCanvasUI_Unloaded(object sender, RoutedEventArgs e)
    {
        RootVisual.IsVisible = false;
    }

    private void WXInkCanvasUI_Loaded(object sender, RoutedEventArgs e)
    {
        RootVisual.IsVisible = true;
    }

    readonly Compositor Compositor;
    readonly ContainerVisual InfiniteVisual;
    readonly ContainerVisual UserOffsetVisual, InternalOffsetVisual;
    readonly ContainerVisual RootVisual;
    readonly Visual RealRootVisual;
}