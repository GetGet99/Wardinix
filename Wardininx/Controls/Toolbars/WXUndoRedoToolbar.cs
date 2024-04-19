#nullable enable
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Get.Symbols;
using Windows.UI.ViewManagement;
using Wardininx.UndoRedos;
namespace Wardininx.Controls.Toolbars;

class WXUndoRedoToolbar(UndoManager UndoManager) : AbstractedUI
{
    public UndoManager UndoManager { get; } = UndoManager;
    protected override UIElement CreateUI() => new WXUndoRedoToolbarUI(this);
}

class WXUndoRedoToolbarUI : WXControl
{
    static UISettings UISettings { get; } = new();
    public WXUndoRedoToolbar Abstracted { get; }
    public WXUndoRedoToolbarUI(WXUndoRedoToolbar abstracted)
    {
        Abstracted = abstracted;
        Template = App.GUIControlTemplate;
    }
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        gui.Content =
            new Border
            {
                Margin = new(16),
                Padding = new(8),
                CornerRadius = new(4),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new AcrylicBrush()
                {
                    TintOpacity = 0.6
                }.AssignTo(out var brush),
                Child = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Resources = {
                        //["ButtonPadding"] = new Thickness(5),
                        [typeof(Button)] = new Style(typeof(Button)) {
                            BasedOn = (Style)Application.Current.Resources[typeof(Button)],
                            Setters =
                            {
                                new Setter { Property = PaddingProperty, Value = new Thickness(5) }
                            }
                        }
                    },
                    Spacing = 4,
                    Children =
                    {
                        new Button() { Content = new SymbolExIcons(SymbolEx.Undo).Build() }.WithCustomCode(x => x.Click += (_, _) => Abstracted.UndoManager.Undo())
                        .WithOneWayBinding(new() {
                            { IsEnabledProperty.AsPropertyDefinition<Button, bool>(), Abstracted.UndoManager.IsUndoableProperty }
                        }),
                        new Button() { Content = new SymbolExIcons(SymbolEx.Redo).Build() }.WithCustomCode(x => x.Click += (_, _) => Abstracted.UndoManager.Redo())
                        .WithOneWayBinding(new() {
                            { IsEnabledProperty.AsPropertyDefinition<Button, bool>(), Abstracted.UndoManager.IsRedoableProperty }
                        }),

                    }
                }
            }
            .WithThemeResources<Border, Color>(x => brush.TintColor = x, "LayerFillColorDefault")
            .WithThemeResources<Border, Color>(x => brush.FallbackColor = x, "SolidBackgroundFillColorBase");

    }
}