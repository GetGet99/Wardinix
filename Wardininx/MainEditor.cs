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
    WXInkCanvas inkCanvas;
    UndoManager UndoManager { get; } = new();
    public MainEditor()
    {
        inkCanvas = new WXInkCanvas(UndoManager);
        inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse;
        inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(new()
        {
            Color = Colors.White,
            Size = new(6, 6)
        });
        inkCanvas.InkPresenter.IsInputEnabled = true;
        var MicaBrush = new BlurredWallpaperMaterials.BackdropMaterial
        {
            Kind = (int)BlurredWallpaperMaterials.BackdropKind.Base,
            Theme = ActualTheme
        };
        ActualThemeChanged += (_1, _2) => MicaBrush.Theme = ActualTheme;
        Content = new Grid
        {
            Background = MicaBrush,
            Children =
            {
                new WXCanvasController { Layers = { inkCanvas } }.UnsafeGetElement<UIElement>(),
                new WXToolbar(UndoManager) {
                    InkToolbar = { InkController = inkCanvas.InkController }
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