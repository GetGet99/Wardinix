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
namespace Wardininx.Controls.Toolbars;

class WXLayerToolbar : AbstractedUI
{
    public UpdateCollection<WXCanvasControl> Layers { get; } = [];
    public Property<int> SelectedIndexProperty = new(0);
    public int SelectedIndex { get => SelectedIndexProperty.Value; set => SelectedIndexProperty.Value = value; }

    public WXLayerToolbar()
    {
        
    }
    protected override UIElement CreateUI() => new WXLayerToolbarUI(this);
}

class WXLayerToolbarUI : WXControl
{
    public WXLayerToolbar Abstracted { get; }
    public WXLayerToolbarUI(WXLayerToolbar abstracted)
    {
        Abstracted = abstracted;
        Template = App.GUIControlTemplate;
    }
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        var gui = (UserControl)GetTemplateChild(App.GUIRootName);
        gui.Content = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                //new WXItemsControlWithIndex<>
                new StackPanel { 
                    Children = {
                        CollectionItemsBinding.Create(Abstracted.Layers.WithIndex(),
                            new DataTemplateWithIndex<WXCanvasControl, UIElement>(
                            root => new ToggleButton
                            {
                                
                            }.WithBinding(
                                new()
                                {
                                    OneWay =
                                    {
                                        {
                                            ContentControl.ContentProperty.AsPropertyDefinition<ToggleButton, object>(),
                                            root.WithForwardConverter(x => x.Index).AssignTo(out var index)
                                            .WithForwardConverter<object>(x => x)
                                        }
                                    },
                                    TwoWay =
                                    {
                                        {
                                            ToggleButton.IsCheckedProperty.AsPropertyDefinition<ToggleButton, bool>(),
                                            index.Combine(Abstracted.SelectedIndexProperty,
                                            (curIdx, selectedIdx) => curIdx == selectedIdx,
                                            delegate (bool isSelected, ref int curIdx, ref int selectedIdx)
                                            {
                                                if (isSelected)
                                                {
                                                    selectedIdx = curIdx;
                                                } else
                                                {
                                                    selectedIdx = curIdx - 1;
                                                }
                                            })
                                        }
                                    }
                                }
                            )
                        ))
                    }
                }
            }
        };
    }
}