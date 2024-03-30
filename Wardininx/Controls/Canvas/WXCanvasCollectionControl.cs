using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Get.Data.XACL;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Get.XAMLTools;
using Windows.Foundation;
using Get.Data.ModelLinker;
using System.Numerics;
using Get.Data.Properties;
using Windows.UI.Xaml;
using Get.Data.Bindings;
namespace Wardininx.Controls.Canvas;
class WXCanvasCollectionControl : WXCanvasControl
{
    public ObservableCollection<WXCanvasControl> Children { get; } = [];
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
        abstracted.CanvasScaleProperty.ValueChanged += (_, value) =>
        {
            foreach (var x in Abstracted.Children) x.CanvasScale = value;
        };
        abstracted.CanvasScrollOffsetProperty.ValueChanged += (_, value) =>
        {
            foreach (var x in Abstracted.Children) x.CanvasScrollOffset = value;
        };
    }

    readonly Dictionary<WXCanvasControl, ValueChangedHandler<Rect>> canvasBoundEventHandlers = [];
    protected override void OnApplyTemplate()
    {
        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        gui.Content = new Grid()
        {
            Children = { CollectionItemsBinding.CreateNonTemplate(Abstracted.Children, x => x.UnsafeGetElement<UIElement>()) },
            Background = new SolidColorBrush(Colors.Transparent)
        };
        var itemsProcessor = new ItemsProccessor<WXCanvasControl>(Abstracted.Children);
        itemsProcessor.ItemsAdded += x =>
        {
            var curBounds = Abstracted.CanvasBounds;
            curBounds.Union(x.CanvasBounds);
            CanvasBoundsWriter.Value = curBounds;
            void ev(Rect oldValue, Rect newValue)
            {
                Rect r = default;
                foreach (var c in Abstracted.Children)
                {
                    if (r == default) r = c.CanvasBounds;
                    else r.Union(c.CanvasBounds);
                }
                CanvasBoundsWriter.Value = r;
            }
            canvasBoundEventHandlers.Add(x, ev);
            x.CanvasBoundsProperty.ValueChanged += ev;
        };
        itemsProcessor.ItemsRemoved += x =>
        {
            var curBounds = Abstracted.CanvasBounds;
            var removedBounds = x.CanvasBounds;
            if (removedBounds.Top == curBounds.Top ||
                removedBounds.Bottom == curBounds.Bottom ||
                removedBounds.Left == curBounds.Left ||
                removedBounds.Right == curBounds.Right)
            {
                Rect r = default;
                foreach (var c in Abstracted.Children)
                {
                    r.Union(c.CanvasBounds);
                }
                CanvasBoundsWriter.Value = r;
            }
        };
        itemsProcessor.Reset();
        base.OnApplyTemplate();
    }
}