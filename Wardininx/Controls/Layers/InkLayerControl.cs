using Get.Data.Helpers;
using Get.Data.Properties;
using System.Numerics;
using Wardininx.API;
using Wardininx.API.Inking;
using Wardininx.Core.Inking;
using Wardininx.Core.Layers;
using Wardininx.UndoRedos;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Wardininx.Controls.Layers;

partial class InkLayerControl : LayerControl
{
    public InkLayerCore Core { get; }
    readonly Property<Rect> CanvasBoundsWriter;
    public InkLayerControl(InkLayerCore core, Document document, Property<Rect> canvasBoundsWriter) : base(core)
    {
        Background = new SolidColorBrush(Colors.Transparent);
        Core = core;
        CanvasBoundsWriter = canvasBoundsWriter;
        // Setup
        RealRootVisual = ElementCompositionPreview.GetElementVisual(this);
        Compositor = RealRootVisual.Compositor;
        RootVisual = Compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(this, RootVisual);
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
        InfiniteVisual.Comment = "InkLayerControl.InfiniteVisual";
        InternalOffsetVisual.Children.InsertAtTop(InfiniteVisual);
        InfiniteVisual.Size = new(InkLayerCore.RealCanvasSize, InkLayerCore.RealCanvasSize);
        InfiniteVisual.Offset = default;
        Core.InkControllerCore.AddHost(InfiniteVisual);
        if (document.IsActiveView)
            Core.InkControllerCore.EditingView = InfiniteVisual;
        document.IsActiveViewProperty.ValueChanged += (_, isActive) =>
        {
            if (isActive) Core.InkControllerCore.EditingView = InfiniteVisual;
        };
        var InkPresenter = Core.InkControllerCore.InkPresenter;
        InkPresenter.StrokesCollected += (_, _) => UpdateBounds();
        InkPresenter.StrokesErased += (_, _) => UpdateBounds();
        void UpdateBounds() {
            var rect = InkPresenter.StrokeContainer.BoundingRect;
            rect.X -= InkLayerCore.RealCanvasSize / 2;
            rect.Y -= InkLayerCore.RealCanvasSize / 2;
            CanvasBoundsWriter.Value = rect;
        }
        // Property Binding
        InfiniteVisual.Offset = Core.CanvasScrollOffset;
        Core.CanvasScrollOffsetProperty.ValueChanged += (_, x) => UserOffsetVisual.Offset = x;
        InfiniteVisual.Offset = Core.CanvasScrollOffset;
        Core.CanvasScaleProperty.ValueChanged += (_, x) => UserOffsetVisual.Scale = x;
        Core.IsSelectedProperty.ValueChanged += (_, @new) =>
        {
            RootVisual.IsHitTestVisible = @new;
        };
        RootVisual.IsHitTestVisible = Core.IsSelected;
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