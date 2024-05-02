#nullable enable
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wardininx.UndoRedos;
using Get.Data.Bindings;
using Wardininx.Controls.Canvas;
using Get.Data.Bindings.Linq;
namespace Wardininx.Controls.Toolbars;

class WXToolbar : AbstractedUI
{
    public UndoManager UndoManager { get; }
    public WXInkToolbar InkToolbar { get; }
    public WXImageToolbar ImageToolbar { get; }
    public WXLayerToolbar LayerToolbar { get; }
    public WXUndoRedoToolbar UndoRedoToolbar { get; }
    public WXToolbar(UndoManager undoManager)
    {
        UndoManager = undoManager;
        InkToolbar = new(this);
        ImageToolbar = new(this);
        LayerToolbar = new(this);
        UndoRedoToolbar = new(undoManager);
        InkToolbar.InkControllerProperty.Bind(
            LayerToolbar.LayersProperty.ElementAt(LayerToolbar.SelectedIndexProperty)
            .Select(x => (x as WXInkCanvas)?.InkController),
            ReadOnlyBindingModes.OneWay
        );
        ImageToolbar.ImageCanvasProperty.Bind(
            LayerToolbar.LayersProperty.ElementAt(LayerToolbar.SelectedIndexProperty)
            .Select(x => x as WXImageCanvas),
            ReadOnlyBindingModes.OneWay
        );
    }
    protected override UIElement CreateUI() => new WXToolbarUI(this);
}

class WXToolbarUI : WXControl
{
    public WXToolbar Abstracted { get; }
    public WXToolbarUI(WXToolbar abstracted)
    {
        Abstracted = abstracted;
        Template = App.GUIControlTemplate;
    }
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        gui.Content = new Grid
        {
            Children =
            {
                Abstracted.InkToolbar.UnsafeGetElement<UIElement>(),
                Abstracted.ImageToolbar.UnsafeGetElement<UIElement>(),
                Abstracted.LayerToolbar.UnsafeGetElement<UIElement>(),
                Abstracted.UndoRedoToolbar.UnsafeGetElement<UIElement>()
            }
        };
    }
}