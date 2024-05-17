using Get.Data.Collections;
using Wardininx.API;
using Wardininx.Controls.Canvas;
using Wardininx.UndoRedos;
using Windows.UI.Xaml.Controls;
namespace Wardininx;
public class MainPage : Page
{
    public MainPage()
    {
        Document document = new(new());
        InkLayerCore CreateInkCanvas() => new(document.UndoManager);
        document.Layers.Add(CreateInkCanvas().GetEditingSession(document));
        Content = new MainEditor(document);
    }
}