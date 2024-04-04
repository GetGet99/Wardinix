using Get.Data.Collections;
using Get.Data.Helpers;
using Get.Data.Properties;
using Get.Data.XACL;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Wardininx.Controls.Canvas;
class WXCanvasController : AbstractedUI
{
    protected override UIElement CreateUI() => new WXCanvasControllerUI(this, CanvasBoundsPropertyProtected);
    public WXCanvasController()
    {
        CanvasBoundsProperty = new(CanvasBoundsPropertyProtected);
    }
    public UpdateCollection<WXCanvasControl> Layers { get; } = [];
    protected readonly Property<Rect> CanvasBoundsPropertyProtected = new(default);
    public static PropertyDefinition<WXCanvasControl, Rect> CanvasBoundsPropertyDefinition { get; }
        = new(x => x.CanvasBoundsProperty);
    public ReadOnlyProperty<Rect> CanvasBoundsProperty { get; }
    public Rect CanvasBounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasBoundsPropertyProtected.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set => CanvasBoundsPropertyProtected.Value = value;
    }
    public Property<float> CanvasScaleProperty { get; } = new(1);
    public float CanvasScale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasScaleProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CanvasScaleProperty.Value = value;
    }
    public Property<Vector3> CanvasScrollOffsetProperty { get; } = new(default);
    public Vector3 CanvasScrollOffset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CanvasScrollOffsetProperty.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CanvasScrollOffsetProperty.Value = value;
    }
    public Property<WXCanvasControlUI?> SelectedCanvasProperty { get; } = new(null);
}
partial class WXCanvasControllerUI : Control
{
    readonly PropertyBase<Vector2> ActualSizeProperty;
    public WXCanvasController Abstracted { get; }
    readonly Property<Rect> CanvasBoundsWriter;
    public WXCanvasControllerUI(WXCanvasController abstracted, Property<Rect> canvasBoundsWriter)
    {
        CanvasBoundsWriter = canvasBoundsWriter;
        Abstracted = abstracted;
        ActualSizeProperty = FrameworkElementProperties.ActualSizePropertyDefinition.GetProperty(this);
        abstracted.SelectedCanvasProperty.ValueChanged += (oldValue, newValue) =>
        {
            // We assume no canvas collection control inside yet!
            oldValue.IsHitTestVisible = false;
            newValue.IsHitTestVisible = true;
        };
        Template = App.GUIControlTemplate;
    }
    static readonly DependencyPropertyDefinition<ScrollBar, double>
        ScrollBarMinimumProperty = RangeBase.MinimumProperty.AsPropertyDefinition<ScrollBar, double>(),
        ScrollBarMaximumProperty = RangeBase.MaximumProperty.AsPropertyDefinition<ScrollBar, double>(),
        ScrollBarViewportSizeProperty = ScrollBar.ViewportSizeProperty.AsPropertyDefinition<ScrollBar, double>(),
        ScrollBarValueProperty = RangeBase.ValueProperty.AsPropertyDefinition<ScrollBar, double>();
    protected override void OnApplyTemplate()
    {
        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        ElementInteractionTracker interactionTracker = null;
        var boundsBinding = CanvasBoundsWriter.ToBinding();
        var actualSizeBinding = ActualSizeProperty.ToBinding();
        var viewOffsetBinding = Abstracted.CanvasScrollOffsetProperty.ToBinding();
        var scaleBinding = Abstracted.CanvasScaleProperty.ToBinding();
        var actualVirtualSizeBinding = actualSizeBinding.Combine(scaleBinding, (x, y) => x / y);
        var visibleRectRegionBinding =
            boundsBinding
            .WithForwardConverter(x => x.Width == 0 && x.Height == 0 ? new Rect(WXInkCanvas.RealCanvasSize / 2, WXInkCanvas.RealCanvasSize / 2, 1, 1) : x)
            .Combine<Vector2, Rect>(
            actualVirtualSizeBinding,
            (bounds, size) => new(Math.Max(0, bounds.Left - size.X), Math.Max(0, bounds.Top - size.Y), bounds.Width + size.X * 2, bounds.Height + size.Y * 2)
        );
        gui.Content = new Grid
        {
            ColumnDefinitions = { new(), new() { Width = GridLength.Auto } },
            RowDefinitions = { new(), new() { Height = GridLength.Auto } },
            Children =
            {
                new WXCanvasCollectionControl()
                {
                    Children = { CollectionItemsBinding.Create(Abstracted.Layers) }
                }
                .WithBinding(new()
                {
                    OneWayToSource =
                    {
                        { WXCanvasControl.CanvasBoundsPropertyDefinition.As<WXCanvasCollectionControl>(), boundsBinding }
                    },
                    OneWay =
                    {
                        {
                            WXCanvasControl.CanvasScrollOffsetPropertyDefinition.As<WXCanvasCollectionControl>(),
                            viewOffsetBinding.WithForwardConverter(x => -x)
                        },
                        {
                            WXCanvasControl.CanvasScalePropertyDefinition.As<WXCanvasCollectionControl>(),
                            scaleBinding.WithForwardConverter(x => new Vector3(x, x, 0))
                        }
                    }
                }).UnsafeGetElement<UIElement>(),
                new ScrollBar
                {
                    IndicatorMode = ScrollingIndicatorMode.MouseIndicator,
                    Orientation = Orientation.Vertical
                }
                .WithBinding(new()
                {
                    OneWay =
                    {
                        { ScrollBarMinimumProperty, visibleRectRegionBinding.WithForwardConverter(x => x.Top) },
                        { ScrollBarMaximumProperty, visibleRectRegionBinding.WithForwardConverter(x => x.Bottom) },
                        { ScrollBarViewportSizeProperty, actualSizeBinding.WithForwardConverter<double>(size => size.Y) }
                    },
                    TwoWay =
                    {
                        { ScrollBarValueProperty, viewOffsetBinding
                            .WithConverter<double>(pos => pos.Y, (y, old) => new(old.X, (float)y, old.Z))
                            .AllowWritebackWhen(() => !interactionTracker.IsInertiaOrInteracting)
                        }
                    }
                })
                .WithCustomCode(x => Grid.SetColumn(x, 1))
                .AssignTo(out var scrollbarY),
                new ScrollBar
                {
                    IndicatorMode = ScrollingIndicatorMode.MouseIndicator,
                    Orientation = Orientation.Horizontal
                }
                .WithBinding(new()
                {
                    OneWay =
                    {
                        { ScrollBarMinimumProperty, visibleRectRegionBinding.WithForwardConverter(x => x.Left) },
                        { ScrollBarMaximumProperty, visibleRectRegionBinding.WithForwardConverter(x => x.Right) },
                        { ScrollBarViewportSizeProperty, actualSizeBinding.WithForwardConverter<double>(size => size.X) },
                    },
                    TwoWay =
                    {
                        { ScrollBarValueProperty, viewOffsetBinding
                            .WithConverter<double>(pos => pos.X, (x, old) => new((float)x, old.Y, old.Z))
                            .AllowWritebackWhen(() => !interactionTracker.IsInertiaOrInteracting)
                        }
                    }
                })
                .WithCustomCode(x => Grid.SetRow(x, 1))
                .AssignTo(out var scrollbarX)
            },
            Background = new SolidColorBrush(Colors.Transparent)
        }
        .WithCustomCode(x => new ElementInteractionTracker(x) { InteractionTracker = { MinScale = 1f, MaxScale = 1f }, ScrollPresenterVisualInteractionSource = { ScaleSourceMode = InteractionSourceMode.Disabled } }
            .WithCustomCode(x =>
                x.InteractionTracker.WithOneWayBinding(new()
                {
                    {
                        PropertyDefinition.CreateExpr<InteractionTracker, Vector3>(
                            it => it.MinPosition, (it, val) => it.MinPosition = val
                        ),
                        visibleRectRegionBinding.WithForwardConverter(x => new Vector3((float)x.Left, (float)x.Top, 0))
                    },
                    {
                        PropertyDefinition.CreateExpr<InteractionTracker, Vector3>(
                            it => it.MaxPosition, (it, val) => it.MaxPosition = val
                        ),
                        visibleRectRegionBinding.WithForwardConverter(x => new Vector3((float)x.Right, (float)x.Bottom, 0))
                    },
                    {
                        PropertyDefinition.CreateExpr<InteractionTracker, Vector3>(
                            it => it.Position, (it, val) => it.TryUpdatePosition(val),
                            () => !x.IsInertiaOrInteracting
                        ),
                        viewOffsetBinding
                    },
                    {
                        PropertyDefinition.CreateExpr<InteractionTracker, float>(
                            it => it.Scale, (it, val) => {
                                var viewport = actualSizeBinding.Value * it.Scale;
                                it.TryUpdateScale(val, viewOffsetBinding.Value + new Vector3(viewport.X, viewport.Y, 0));
                            }, () => !x.IsInertiaOrInteracting
                        ),
                        scaleBinding
                    },
                })
            )
            .WithCustomCode(
                x => x.ValuesChangedEvent += args =>
                {
                    if (x.IsInertiaOrInteracting)
                    {
                        Abstracted.CanvasScrollOffsetProperty.Value = args.Position;
                        Abstracted.CanvasScale = args.Scale;
                    }
                }
            ),
            out interactionTracker
        )
        .WithCustomCode(x => x.PointerWheelChanged += (_, e) =>
        {
            // TODO: Setup Animations
            var pointerProp = e.GetCurrentPoint(x).Properties;
            var pos = Abstracted.CanvasScrollOffsetProperty.Value;
            if (pointerProp.IsHorizontalMouseWheel || e.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Shift))
            {
                pos = pos with { X = pos.X - pointerProp.MouseWheelDelta * scaleBinding.Value };
            }
            else
                pos = pos with { Y = pos.Y - pointerProp.MouseWheelDelta * scaleBinding.Value };
            Abstracted.CanvasScrollOffsetProperty.Value = pos;
        });
        base.OnApplyTemplate();
    }
}