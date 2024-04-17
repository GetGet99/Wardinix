using Get.Data.Collections;
using Wardininx.Controls.Canvas;
using Wardininx.Controls.Toolbars;
using Wardininx.UndoRedos;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        UpdateCollectionInitializer<WXCanvasControl> Layers = [CreateInkCanvas(), CreateInkCanvas(), CreateInkCanvas()];
        Content = new Grid
        {
            Background = MicaBrush,
            Children =
            {
                new WXCanvasController { Layers = Layers }.UnsafeGetElement<UIElement>(),
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