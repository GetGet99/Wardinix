#nullable enable
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wardininx.UndoRedos;
namespace Wardininx.Controls.Toolbars;

class WXToolbar : AbstractedUI
{
    public UndoManager UndoManager { get; }
    public WXInkToolbar InkToolbar { get; }
    public WXToolbar(UndoManager undoManager)
    {
        UndoManager = undoManager;
        InkToolbar = new(this);
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
                Abstracted.InkToolbar.UnsafeGetElement<UIElement>()
            }
        };
    }
}