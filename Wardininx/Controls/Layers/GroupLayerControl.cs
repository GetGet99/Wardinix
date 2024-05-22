using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Foundation;
using Windows.UI.Xaml;
using Get.Data.Collections;
using Get.Data.Collections.Linq;
using Get.Data.Collections.Update;
using Wardininx.Core.Layers;
using Wardininx.API;
namespace Wardininx.Controls.Layers;
partial class GroupLayerControl : LayerControl
{
    public GroupLayerCore Core { get; }
    readonly Property<Rect> CanvasBoundsWriter;
    readonly Document document;
    public GroupLayerControl(GroupLayerCore core, Document document, Property<Rect> canvasBoundsWriter) : base(core)
    {
        Core = core;
        CanvasBoundsWriter = canvasBoundsWriter;
        Template = App.GUIControlTemplate;
        var transform = new CompositeTransform();
        RenderTransform = transform;
        core.CanvasScaleProperty.ValueChanged += (_, value) =>
        {
            transform.ScaleX = value.X;
            transform.ScaleY = value.Y;
        };
        core.CanvasScrollOffsetProperty.ValueChanged += (_, value) =>
        {
            transform.TranslateX = value.X;
            transform.TranslateY = value.Y;
        };
        this.document = document;
    }

    readonly Dictionary<LayerCore, ValueChangedHandler<Rect>> canvasBoundEventHandlers = [];
    protected override void OnApplyTemplate()
    {
        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        gui.Content = new Grid()
        {
            Children = { Core.Children.AsUpdate().Select(x => x.UnsafeGetElement<UIElement>(document)) },
            Background = new SolidColorBrush(Colors.Transparent)
        };
        Core.Children.ItemsChanged += (actions) =>
        {
            foreach (var action in actions)
            {
                if (action is ItemsAddedUpdateAction<LayerCore> added)
                    OnItemsAdded(added.Items.AsEnumerable());
                if (action is ItemsRemovedUpdateAction<LayerCore> removed)
                {
                    var xs = removed.Items.AsEnumerable();
                    foreach (var x in xs)
                    {
                        if (canvasBoundEventHandlers.Remove(x, out var ev))
                            x.CanvasBoundsProperty.ValueChanged -= ev;
                        var curBounds = Core.CanvasBounds;
                        var removedBounds = x.CanvasBounds;
                        if (removedBounds.Top == curBounds.Top ||
                            removedBounds.Bottom == curBounds.Bottom ||
                            removedBounds.Left == curBounds.Left ||
                            removedBounds.Right == curBounds.Right)
                        {
                            Rect r = default;
                            if (Core.Children.Count > 1)
                            {
                                r = Core.Children[0].CanvasBounds;
                            }
                            foreach (var c in Core.Children.AsUpdate().AsEnumerable())
                            {
                                if (c.CanvasBounds != default) r.Union(c.CanvasBounds);
                            }
                            CanvasBoundsWriter.Value = r;
                        }
                    }
                }
            }
        };
        void OnItemsAdded(IEnumerable<LayerCore> xs)
        {
            foreach (var x in xs)
            {
                var curBounds = Core.CanvasBounds;
                curBounds.Union(x.CanvasBounds);
                CanvasBoundsWriter.Value = curBounds;
                void ev(Rect oldValue, Rect newValue)
                {
                    Rect r = default;
                    if (Core.Children.Count > 1)
                    {
                        r = Core.Children[0].CanvasBounds;
                    }
                    foreach (var c in Core.Children.AsUpdate().AsEnumerable())
                    {
                        if (c.CanvasBounds != default) r.Union(c.CanvasBounds);
                    }
                    CanvasBoundsWriter.Value = r;
                }
                canvasBoundEventHandlers.Add(x, ev);
                x.CanvasBoundsProperty.ValueChanged += ev;
            }
        }
        OnItemsAdded(Core.Children.AsUpdate().AsEnumerable());
        base.OnApplyTemplate();
    }
}