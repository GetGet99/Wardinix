using Get.Data.Bindings;
using Get.Data.Collections;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Drawing;
using System.Numerics;
using Wardininx.Controls;
using Wardininx.Controls.Canvas;
using Wardininx.Controls.Toolbars;
using Wardininx.UndoRedos;
using Windows.UI;
using Windows.UI.Composition.Interactions;
using Windows.UI.Input.Inking.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Wardininx;

class MainEditor : UserControl
{
    UndoManager UndoManager { get; } = new();
    public MainEditor()
    {
        WXInkCanvas CreateInkCanvas()
        {
            WXInkCanvas inkCanvas = new WXInkCanvas(UndoManager);
            return inkCanvas;
        }
        var MicaBrush = new BlurredWallpaperMaterials.BackdropMaterial
        {
            Kind = (int)BlurredWallpaperMaterials.BackdropKind.Base,
            Theme = ActualTheme
        };
        ActualThemeChanged += (_1, _2) => MicaBrush.Theme = ActualTheme;
        UpdateCollection<WXCanvasControl> Layers = [CreateInkCanvas(), CreateInkCanvas(), CreateInkCanvas()];
        Layers.ItemsAdded += delegate
        {
            var a = Layers;
        };
        Content = new Grid
        {
            Background = MicaBrush,
            Children =
            {
                new WXCanvasController { Layers = Layers.AsReadOnly() }.UnsafeGetElement<UIElement>(),
                new WXToolbar(UndoManager) {
                    LayerToolbar = { Layers = Layers, SelectedIndex = 0 }
                }.UnsafeGetElement<UIElement>()
            }
        };
    }

}
public class MainPage : Page
{
    public MainPage()
    {
        Content = new MainEditor();
    }
}