using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Foundation;
using Windows.UI.Xaml;
using Get.Data.Collections;
using Get.Data.Collections.Linq;
using Get.Data.Collections.Update;
namespace Wardininx.Controls.Canvas;
class WXCanvasCollectionControl : WXCanvasControl
{
    public TwoWayUpdateCollectionProperty<WXCanvasControl> Children { get; } = new();
    protected override UIElement CreateUI() => new WXCanvasCollectionControlUI(this, CanvasBoundsPropertyProtected);
}
partial class WXCanvasCollectionControlUI : WXCanvasControlUI
{
    public WXCanvasCollectionControl Abstracted { get; }
    readonly Property<Rect> CanvasBoundsWriter;
    public WXCanvasCollectionControlUI(WXCanvasCollectionControl abstracted, Property<Rect> canvasBoundsWriter)
    {
        Abstracted = abstracted;
        CanvasBoundsWriter = canvasBoundsWriter;
        Template = App.GUIControlTemplate;
        var transform = new CompositeTransform();
        RenderTransform = transform;
        abstracted.CanvasScaleProperty.ValueChanged += (_, value) =>
        {
            transform.ScaleX = value.X;
            transform.ScaleY = value.Y;
        };
        abstracted.CanvasScrollOffsetProperty.ValueChanged += (_, value) =>
        {
            transform.TranslateX = value.X;
            transform.TranslateY = value.Y;
        };
    }

    readonly Dictionary<WXCanvasControl, ValueChangedHandler<Rect>> canvasBoundEventHandlers = [];
    protected override void OnApplyTemplate()
    {
        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        gui.Content = new Grid()
        {
            Children = { Abstracted.Children.AsUpdate().Select(x => x.UnsafeGetElement<UIElement>()) },
            Background = new SolidColorBrush(Colors.Transparent)
        };
        Abstracted.Children.ItemsChanged += (actions) =>
        {
            foreach (var action in actions)
            {
                if (action is ItemsAddedUpdateAction<WXCanvasControl> added)
                    OnItemsAdded(added.Items.AsEnumerable());
                if (action is ItemsRemovedUpdateAction<WXCanvasControl> removed)
                {
                    var xs = removed.Items.AsEnumerable();
                    foreach (var x in xs)
                    {
                        if (canvasBoundEventHandlers.Remove(x, out var ev))
                            x.CanvasBoundsProperty.ValueChanged -= ev;
                        var curBounds = Abstracted.CanvasBounds;
                        var removedBounds = x.CanvasBounds;
                        if (removedBounds.Top == curBounds.Top ||
                            removedBounds.Bottom == curBounds.Bottom ||
                            removedBounds.Left == curBounds.Left ||
                            removedBounds.Right == curBounds.Right)
                        {
                            Rect r = default;
                            if (Abstracted.Children.Count > 1)
                            {
                                r = Abstracted.Children[0].CanvasBounds;
                            }
                            foreach (var c in Abstracted.Children.AsUpdate().AsEnumerable())
                            {
                                if (c.CanvasBounds != default) r.Union(c.CanvasBounds);
                            }
                            CanvasBoundsWriter.Value = r;
                        }
                    }
                }
            }
        };
        void OnItemsAdded(IEnumerable<WXCanvasControl> xs)
        {
            foreach (var x in xs)
            {
                var curBounds = Abstracted.CanvasBounds;
                curBounds.Union(x.CanvasBounds);
                CanvasBoundsWriter.Value = curBounds;
                void ev(Rect oldValue, Rect newValue)
                {
                    Rect r = default;
                    if (Abstracted.Children.Count > 1)
                    {
                        r = Abstracted.Children[0].CanvasBounds;
                    }
                    foreach (var c in Abstracted.Children.AsUpdate().AsEnumerable())
                    {
                        if (c.CanvasBounds != default) r.Union(c.CanvasBounds);
                    }
                    CanvasBoundsWriter.Value = r;
                }
                canvasBoundEventHandlers.Add(x, ev);
                x.CanvasBoundsProperty.ValueChanged += ev;
            }
        }
        OnItemsAdded(Abstracted.Children.AsUpdate().AsEnumerable());
        base.OnApplyTemplate();
    }
}