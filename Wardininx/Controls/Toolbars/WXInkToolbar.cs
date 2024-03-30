#nullable enable
using Get.Data.Helpers;
using Get.Data.Properties;
using Get.Data.Bindings;
using Get.Data.XACL;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Wardininx.Classes;
using Get.Symbols;
namespace Wardininx.Controls.Toolbars;

class WXInkToolbar : AbstractedUI
{
    public WXToolbar Parent { get; }
    public Property<WXInkController?> InkControllerProperty { get; } = new(null);
    public WXInkToolbar(WXToolbar parent)
    {
        Parent = parent;
        PenColorProperty.Bind(
            Binding<Color>.Create(FavoritePenColors, PenColorIndexProperty),
            BindingModes.OneWay
        );
        HighlighterColorProperty.Bind(
            Binding<Color>.Create(FavoriteHighlighterColors, HighlighterColorIndexProperty),
            BindingModes.OneWay
        );
        //PenSizeProperty.Bind(
        //    Binding<double>.Create(FavoritePenSizes, PenSizeIndexProperty),
        //    BindingModes.OneWay
        //);
        //HighlighterSizeProperty.Bind(
        //    Binding<double>.Create(FavoriteHighlighterSizes, HighlighterSizeIndexProperty),
        //    BindingModes.OneWay
        //);

        InkControllerProperty.ValueChanged += (_, _) => UpdateDrawingAttribute();
        PenColorProperty.ValueChanged += (_, _) => UpdateDrawingAttribute();
        HighlighterColorProperty.ValueChanged += (_, _) => UpdateDrawingAttribute();
        PenSizeProperty.ValueChanged += (_, _) => UpdateDrawingAttribute();
        HighlighterSizeProperty.ValueChanged += (_, _) => UpdateDrawingAttribute();
        EditingModeProperty.ValueChanged += (_, _) => UpdateDrawingAttribute();

    }
    void UpdateDrawingAttribute()
    {
        var inkController = InkController;
        if (inkController is null) return;

        InkDrawingAttributes attr;
        switch (EditingMode)
        {
            case EditingModes.Pen:
                inkController.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
                attr = new()
                {
                    Size = new(PenSizeProperty.Value, PenSizeProperty.Value),
                    Color = PenColorProperty.Value
                };
                inkController.UpdateDefaultDrawingAttributes(attr);
                break;
            case EditingModes.Pencil:
                inkController.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
                attr = InkDrawingAttributes.CreateForPencil();
                attr.PencilProperties.Opacity = 1;
                attr.Size = new(PenSizeProperty.Value, PenSizeProperty.Value);
                attr.Color = PenColorProperty.Value;
                inkController.UpdateDefaultDrawingAttributes(attr);
                break;
            case EditingModes.Highlighter:
                inkController.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
                attr = new()
                {
                    DrawAsHighlighter = true,
                    Size = new(HighlighterSizeProperty.Value, HighlighterSizeProperty.Value),
                    Color = HighlighterColorProperty.Value
                };
                inkController.UpdateDefaultDrawingAttributes(attr);
                break;
            case EditingModes.Eraser:
                inkController.InputProcessingConfiguration.Mode = InkInputProcessingMode.Erasing;
                break;
            case EditingModes.LassoSelect:
            case EditingModes.RectSelect:
                inkController.InputProcessingConfiguration.Mode = InkInputProcessingMode.None;
                break;
        }
    }
    public WXInkController? InkController { get => InkControllerProperty.Value; set => InkControllerProperty.Value = value; }
    protected override UIElement CreateUI() => new WXInkToolbarUI(this);
    public Property<Color> PenColorProperty { get; } = new(Colors.White);
    public Property<Color> HighlighterColorProperty { get; } = new(Colors.Yellow);
    public Property<double> PenSizeProperty { get; } = new(10);
    public Property<double> HighlighterSizeProperty { get; } = new(30);
    public Property<EditingModes> EditingModeProperty { get; } = new(EditingModes.Pen);
    public EditingModes EditingMode { get => EditingModeProperty.Value; set => EditingModeProperty.Value = value; }
    public enum EditingModes
    {
        Pen,
        Pencil,
        Highlighter,
        Eraser,
        LassoSelect,
        RectSelect
    }
    public ObservableCollection<Color> FavoritePenColors { get; } = [
        Colors.Black,
        Colors.White,
        Color.FromArgb(255, 192, 29, 29), // red
        Color.FromArgb(255, 212, 172, 8), // yellow/orange
        Color.FromArgb(255, 38, 192, 8),  // green
        Color.FromArgb(255, 8, 144, 171), // cyan
        Color.FromArgb(255, 101, 0, 203), // purple
    ];
    public Property<int> PenColorIndexProperty { get; } = new(default);
    public ObservableCollection<double> FavoritePenSizes { get; } = [];
    public Property<int> PenSizeIndexProperty { get; } = new(default);
    public ObservableCollection<Color> FavoriteHighlighterColors { get; } = [
        Color.FromArgb(255, 235, 0, 139), // pink highlight
        Color.FromArgb(255, 255, 85, 0),  // orange highlight
        Color.FromArgb(255, 192, 29, 29), // red
        Color.FromArgb(255, 255, 230, 0), // yellow highlight
        Color.FromArgb(255, 38, 230, 0),  // green highlight
        Color.FromArgb(255, 68, 200, 245), // cyan highlight
        Color.FromArgb(255, 102, 0, 204), // purple highlight
    ];
    public Property<int> HighlighterColorIndexProperty { get; } = new(default);
    public ObservableCollection<double> FavoriteHighlighterSizes { get; } = [];
    public Property<int> HighlighterSizeIndexProperty { get; } = new(default);
}

class WXInkToolbarUI : WXControl
{
    public WXInkToolbar Abstracted { get; }
    public WXInkToolbarUI(WXInkToolbar abstracted)
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
                HorizontalAlignment = HorizontalAlignment.Center,
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
                            new Button() { Content = new SymbolExIcons(SymbolEx.Marker).Build() }.WithCustomCode(x => x.Click += (_, _) => Abstracted.EditingMode = WXInkToolbar.EditingModes.Pen),
                            new Button() { Content = new SymbolExIcons(SymbolEx.Pencil).Build() }.WithCustomCode(x => x.Click += (_, _) => Abstracted.EditingMode = WXInkToolbar.EditingModes.Pencil),
                            new Button() { Content = new SymbolExIcons(SymbolEx.Highlight).Build() }.WithCustomCode(x => x.Click += (_, _) => Abstracted.EditingMode = WXInkToolbar.EditingModes.Highlighter),
                            new Button() { Content = new SymbolExIcons(SymbolEx.StrokeErase).Build() }.WithCustomCode(x => x.Click += (_, _) => Abstracted.EditingMode = WXInkToolbar.EditingModes.Eraser),
                            //new Button() { Content = "L" }.WithCustomCode(x => x.Click += (_, _) => Abstracted.EditingMode = WXInkToolbar.EditingModes.LassoSelect),
                            //new Button() { Content = "R" }.WithCustomCode(x => x.Click += (_, _) => Abstracted.EditingMode = WXInkToolbar.EditingModes.RectSelect),
                            new Button() { Content = new SymbolExIcons(SymbolEx.Undo).Build() }.WithCustomCode(x => x.Click += (_, _) => Abstracted.Parent.UndoManager.Undo())
                            .WithOneWayBinding(new() {
                                { IsEnabledProperty.AsPropertyDefinition<Button, bool>(), Abstracted.Parent.UndoManager.IsUndoableProperty }
                            }),
                            new Button() { Content = new SymbolExIcons(SymbolEx.Redo).Build() }.WithCustomCode(x => x.Click += (_, _) => Abstracted.Parent.UndoManager.Redo())
                            .WithOneWayBinding(new() {
                                { IsEnabledProperty.AsPropertyDefinition<Button, bool>(), Abstracted.Parent.UndoManager.IsRedoableProperty }
                            }),
                            new Border()
                            .WithCustomCode(x => ((colors, setter) =>
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 4,
                                Resources =
                                {
                                    ["ButtonForeground"] = new SolidColorBrush(Colors.Transparent),
                                    ["ButtonForegroundPointerOver"] = new SolidColorBrush(Colors.Transparent),
                                    ["ButtonForegroundPressed"] = new SolidColorBrush(Colors.Transparent),
                                },
                                Children =
                                {
                                    CollectionItemsBinding.Create(colors, new DataTemplate<(int Index, Color Value), UIElement>(
                                        IdxColor => new Button() {
                                            Width = 30, Height = 30,
                                            CornerRadius = new(15),
                                            Background = new SolidColorBrush().AssignTo(out var brush),
                                            Resources =
                                            {
                                                ["ButtonBackground"] = brush,
                                                ["ButtonBackgroundPointerOver"] = new SolidColorBrush() { Opacity = 0.9 }.AssignTo(out var brush2),
                                                ["ButtonBackgroundPressed"] = new SolidColorBrush() { Opacity = 0.8 }.AssignTo(out var brush3)
                                            }
                                        }
                                        .WithOneWayBinding(new()
                                        {
                                            { PropertyDefinition.CreateExpr<Button, Color>(_ => brush.Color, (_, val) => brush.Color = brush2.Color = brush3.Color = val), IdxColor.WithForwardConverter(x => x.Value) }
                                        })
                                        .WithCustomCode(
                                            x => x.Click += (_, _) => setter(IdxColor.Value.Index)
                                        )
                                    ))
                                }
                            }), out Func<ObservableCollection<Color>, Action<int>, StackPanel> panelCreator)
                            .WithCustomCode(x =>
                                (penColorEditor: panelCreator(Abstracted.FavoritePenColors, x => Abstracted.PenColorIndexProperty.Value = x),
                                highlightColorEditor: panelCreator(Abstracted.FavoriteHighlighterColors, x => Abstracted.HighlighterColorIndexProperty.Value = x)),
                                out var a
                            )
                            .WithOneWayBinding(new()
                            {
                                { PropertyDefinition.CreateExpr<Border, UIElement>(x => x.Child, (x, val) => x.Child = val),
                                    Abstracted.EditingModeProperty.ToBinding().WithForwardConverter(x => (UIElement)(x is WXInkToolbar.EditingModes.Highlighter ? a.highlightColorEditor : a.penColorEditor)) }
                            })
                        }
                }
            }
            .WithThemeResources<Border, Color>(x => brush.TintColor = x, "LayerFillColorDefault")
            .WithThemeResources<Border, Color>(x => brush.FallbackColor = x, "SolidBackgroundFillColorBase");
    }
}