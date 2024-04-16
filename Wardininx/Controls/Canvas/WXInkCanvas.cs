using Get.Data.Helpers;
using Get.Data.Properties;
using System.Numerics;
using System.Runtime.CompilerServices;
using Wardininx.Classes;
using Wardininx.UndoRedos;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Wardininx.Controls.Canvas;

partial class WXInkCanvas : WXCanvasControl
{
    public WXInkController InkController { get; }
    public WXInkCanvas(UndoManager undoManager)
    {
        InkController = new(undoManager, InkPresenter);
        InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen; //  | Windows.UI.Core.CoreInputDeviceTypes.Touch 
        InkPresenter.IsInputEnabled = true;
    }

    public const float RealCanvasSize = 100000; // 1 << 20;
    CoreInkPresenterHost CoreInkPresenterHost { get; } = new();
    public InkPresenter InkPresenter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CoreInkPresenterHost.InkPresenter;
    }
    protected override UIElement CreateUI() => new WXInkCanvasUI(this, CoreInkPresenterHost, CanvasBoundsPropertyProtected);
}
partial class WXInkCanvasUI : WXCanvasControlUI
{
    public WXInkCanvas Abstracted { get; }
    readonly CoreInkPresenterHost CoreInkPresenterHost;
    readonly Property<Rect> CanvasBoundsWriter;
    public WXInkCanvasUI(WXInkCanvas abstracted, CoreInkPresenterHost coreInkPresenterHost, Property<Rect> canvasBoundsWriter)
    {
        Background = new SolidColorBrush(Colors.Transparent);
        Abstracted = abstracted;
        CoreInkPresenterHost = coreInkPresenterHost;
        CanvasBoundsWriter = canvasBoundsWriter;
        // Setup
        RealRootVisual = ElementCompositionPreview.GetElementVisual(this);
        Compositor = RealRootVisual.Compositor;
        RootVisual = Compositor.CreateContainerVisual();
        ElementCompositionPreview.SetElementChildVisual(this, RootVisual);
        RootVisual.Clip = Compositor.CreateInsetClip(0, 0, 0, 0);
        RootVisual.RelativeSizeAdjustment = Vector2.One;
        UserOffsetVisual = Compositor.CreateContainerVisual();
        UserOffsetVisual.Size = new(WXInkCanvas.RealCanvasSize, WXInkCanvas.RealCanvasSize);
        UserOffsetVisual.Offset = default;
        RootVisual.Children.InsertAtTop(UserOffsetVisual);
        InternalOffsetVisual = Compositor.CreateContainerVisual();
        InternalOffsetVisual.Size = new(WXInkCanvas.RealCanvasSize, WXInkCanvas.RealCanvasSize);
        InternalOffsetVisual.Offset = new(-WXInkCanvas.RealCanvasSize / 2, -WXInkCanvas.RealCanvasSize / 2, 0);
        UserOffsetVisual.Children.InsertAtTop(InternalOffsetVisual);
        InfiniteVisual = Compositor.CreateContainerVisual();
        InternalOffsetVisual.Children.InsertAtTop(InfiniteVisual);
        InfiniteVisual.Size = new(WXInkCanvas.RealCanvasSize, WXInkCanvas.RealCanvasSize);
        InfiniteVisual.Offset = default;
        CoreInkPresenterHost.RootVisual = InfiniteVisual;
        Abstracted.InkPresenter.StrokesCollected += (_, _) => UpdateBounds();
        Abstracted.InkPresenter.StrokesErased += (_, _) => UpdateBounds();
        void UpdateBounds() {
            var rect = Abstracted.InkPresenter.StrokeContainer.BoundingRect;
            rect.X -= WXInkCanvas.RealCanvasSize / 2;
            rect.Y -= WXInkCanvas.RealCanvasSize / 2;
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
    }
    readonly Compositor Compositor;
    readonly ContainerVisual InfiniteVisual;
    readonly ContainerVisual UserOffsetVisual, InternalOffsetVisual;
    readonly ContainerVisual RootVisual;
    readonly Visual RealRootVisual;
}