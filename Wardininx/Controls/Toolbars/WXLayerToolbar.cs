#nullable enable
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wardininx.UndoRedos;
using Wardininx.Controls.Canvas;
using Get.Data.Bindings;
using Get.Data.XACL;
using Get.Data.Properties;
using Get.Data.Helpers;
using Windows.UI.Xaml.Controls.Primitives;
using Get.Data.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
namespace Wardininx.Controls.Toolbars;

class WXLayerToolbar : AbstractedUI
{
    public OneWayUpdateCollectionProperty<WXCanvasControl> LayersProperty { get; } = [];
    public IReadOnlyUpdateCollection<WXCanvasControl> Layers { get => LayersProperty.Value; set => LayersProperty.Value = value; }
    public Property<int> SelectedIndexProperty { get; } = new(-1);
    public int SelectedIndex { get => SelectedIndexProperty.Value; set => SelectedIndexProperty.Value = value; }
    public Property<WXCanvasControl?> SelectedLayerProperty { get; } = new(null);
    public WXLayerToolbar()
    {
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
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new WXSelectableItemsControl<WXCanvasControl>(
                    new StackPanel { Orientation = Orientation.Vertical, Spacing = 4,
                    Margin = new(16),
                    Padding = new(8),
                    CornerRadius = new(4),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Background = new AcrylicBrush()
                    {
                        TintOpacity = 0.6
                    }
                }.AssignTo(out var sp), sp.Children
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
                                        root.To(x => x.IndexItemBinding).WithForwardConverter(x => (object)x.Index)
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
                    { WXSelectableItemsControl<WXCanvasControl>.SelectedIndexPropertyDefnition, Abstracted.SelectedIndexProperty }
                }).WithOneWayToSourceBinding(new()
                {
                    { WXSelectableItemsControl<WXCanvasControl>.SelectedValuePropertyDefnition, Abstracted.SelectedLayerProperty }
                }),
            }
        };
    }
}