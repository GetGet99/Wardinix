using Get.Data.Collections;
using Wardininx.API;
using Wardininx.Controls.Layers;
using Wardininx.Core.Layers;
using Wardininx.UndoRedos;
using Windows.UI.Xaml.Controls;
namespace Wardininx;
public class MainPage : Page
{
    public MainPage()
    {
        Document document = new(new());
        document.Layers.Add(new InkLayerCore().GetEditingSession(document));
        Content = new MainEditor(document);
    }
}