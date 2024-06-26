#nullable enable
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Get.Data.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Get.Symbols;
using Windows.UI;
using Get.Data.Collections.Update;
using Get.Data.Bindings.Linq;
using Get.Data.Collections.Conversion;
using Get.Data.Collections.Linq;
using Wardininx.API;
using Wardininx.Core.Layers;
namespace Wardininx.Controls.Toolbars;

class WXLayerToolbar : AbstractedUI
{
    public TwoWayUpdateCollectionProperty<ILayer> LayersProperty { get; } = [];
    public IUpdateCollection<ILayer> Layers { get => LayersProperty.Value; set => LayersProperty.Value = value; }
    public Property<int> SelectedIndexProperty { get; } = new(-1) { DebugName = "LayerToolbar.SelectedIndex" };
    public int SelectedIndex { get => SelectedIndexProperty.Value; set => SelectedIndexProperty.Value = value; }
    public Property<ILayer?> SelectedLayerProperty { get; } = new(null);
    public WXToolbar Parent { get; }
    public Document Document { get; }
    public WXLayerToolbar(WXToolbar toolbar, Document doc)
    {
        Document = doc;
        Parent = toolbar;
        SelectedLayerProperty.ValueChanged += (old, @new) =>
        {
            if (old is not null)
                old.IsSelected = false;
            if (@new is not null)
                @new.IsSelected = true;
        };
    }
    protected override UIElement CreateUI() => new WXLayerToolbarUI(this);
}

class WXLayerToolbarUI : WXControl
{
    UISettings UISettings { get; } = new();
    public WXLayerToolbar Abstracted { get; }
    public WXLayerToolbarUI(WXLayerToolbar abstracted)
    {
        Abstracted = abstracted;
        Template = App.GUIControlTemplate;
    }
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var selectedBorderBrush = new SolidColorBrush(UISettings.GetColorValue(UIColorType.Accent));
        UISettings.ColorValuesChanged += delegate
        {
            selectedBorderBrush.Color = UISettings.GetColorValue(UIColorType.Accent);
        };
        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        gui.Content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 4,
            Margin = new(16),
            Padding = new(8),
            CornerRadius = new(4),
            Background = new AcrylicBrush()
            {
                TintOpacity = 0.6
            }.AssignTo(out var brush),
            Children =
            {
                new Button()
                {
                    Content = new SymbolExIcons(SymbolEx.Add).Build(),
                    Width = 35,
                    Height = 35,
                    Padding = new(5)
                }.WithCustomCode(x => x.Click += delegate
                {

                    var idx = Abstracted.LayersProperty.Count;
                    Abstracted.Document.Layers.AddSelect(new InkLayerCore());
                }),
                new WXSelectableItemsControl<ILayer>(
                    new StackPanel {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Spacing = 4,
                    }.AssignTo(out var sp), sp.Children.AsGDCollection().Reverse()
                )
                {
                    ItemsSource = Abstracted.LayersProperty,
                    ItemTemplate = new(
                        root => new Button
                        {
                            Width = 36,
                            Height = 36
                        }.WithBinding(
                            new()
                            {
                                OneWay =
                                {
                                    {
                                        ContentControl.ContentProperty.AsPropertyDefinition<Button, object>(),
                                        root.SelectPath(x => x.IndexItemBinding).Select(x =>
                                        {
                                            var r = root;
                                            return (object)(x.Index + 1);
                                        })
                                    }
                                }
                            }
                        ).WithCustomCode(x =>
                        {
                            x.Click += (_, _) => root.CurrentValue.Select();

                            var defaultBorderBrush = (Brush)x.Resources["ButtonBorderBrush"];
                            // NOT DYNAMIC
                            x.Resources["ButtonBorderBrushPressed"] = x.Resources["ButtonBorderBrushPointerOver"]
                                = selectedBorderBrush;
                            void Set(bool @new)
                            {
                                if (@new)
                                {
                                    x.BorderBrush = selectedBorderBrush;
                                }
                                else
                                {
                                    x.BorderBrush = defaultBorderBrush;
                                }
                            }
                            root.CurrentValue.IsSelected.ValueChanged += (_, @new) => Set(@new);
                            Set(root.CurrentValue.IsSelected.CurrentValue);
                        })
                    )
                }.WithTwoWayBinding(new()
                {
                    { WXSelectableItemsControl<ILayer>.SelectedIndexPropertyDefnition, Abstracted.SelectedIndexProperty }
                }).WithOneWayToSourceBinding(new()
                {
                    { WXSelectableItemsControl<ILayer>.SelectedValuePropertyDefnition, Abstracted.SelectedLayerProperty }
                })
            }
        };
        this
        .WithThemeResources<FrameworkElement, Color>(x => brush.TintColor = x, "LayerFillColorDefault")
        .WithThemeResources<FrameworkElement, Color>(x => brush.FallbackColor = x, "SolidBackgroundFillColorBase");
    }
}