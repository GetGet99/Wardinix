using Get.Data.Collections;
using Get.Data.Helpers;
using Get.Data.Properties;
using Get.Data.XACL;
using System.Text;
using Wardininx.Classes;
using Wardininx.Controls.Canvas;
using Wardininx.Controls.Toolbars;
using Wardininx.UndoRedos;
using Windows.Data.Json;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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
        UpdateCollectionInitializer<WXCanvasControl> Layers = [CreateInkCanvas()];
        Property<int> SelectedIndexProperty = new(0);
        Content = new Grid
        {
            Background = MicaBrush,
            Children =
            {
                new WXCanvasController { Layers = Layers }.UnsafeGetElement<UIElement>(),
                new WXToolbar(UndoManager) {
                    LayerToolbar = { Layers = Layers }
                }
                .AssignTo(out var toolbar)
                .UnsafeGetElement<UIElement>()
            }
        };
        toolbar.LayerToolbar.SelectedIndexProperty.Bind(SelectedIndexProperty, Get.Data.Bindings.ReadOnlyBindingModes.OneWay);
        KeyDown += async (o, e) =>
        {
            if (e.Key == Windows.System.VirtualKey.S)
            {
                var filePicker = new FileSavePicker()
                {
                    DefaultFileExtension = ".wdii",
                    FileTypeChoices =
                {
                    { "Wardinix file", [".wdii"] }
                }
                };
                var file = await filePicker.PickSaveFileAsync();
                if (file is null) return;
                var jsonArr = new JsonArray();
                foreach (var layer in Layers)
                {
                    switch (layer)
                    {
                        case WXInkCanvas inkCanvas:
                            {
                                using var ms = new MemoryStream();
                                await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(ms.AsRandomAccessStream());
                                jsonArr.Add(new JsonObject
                                {
                                    { "LayerType", JsonValue.CreateStringValue("Ink") },
                                    { "InkData", JsonValue.CreateStringValue(Convert.ToBase64String(ms.ToArray())) }
                                });
                            }
                            break;
                        case WXImageCanvas imageCanvas:
                            {
                                jsonArr.Add(new JsonObject
                                {
                                    { "LayerType", JsonValue.CreateStringValue("Image") },
                                    { "ImageData", JsonValue.CreateStringValue(Convert.ToBase64String(imageCanvas.Image.ToBytes())) }
                                });
                            }
                            break;
                    }
                }
                using var stream = await file.OpenStreamForWriteAsync();
                stream.Position = 0;
                using StreamWriter writer = new(stream);
                await writer.WriteAsync(
                    new JsonObject()
                    {
                    { "Type", JsonValue.CreateStringValue("LayerContainer") },
                    { "Layers", jsonArr }
                    }.ToString()
                );
            }
            else if (e.Key == Windows.System.VirtualKey.O)
            {
                var filePicker = new FileOpenPicker()
                {
                    FileTypeFilter = { ".wdii" }
                };
                var file = await filePicker.PickSingleFileAsync();
                if (file is null) return;
                var or = await file.OpenReadAsync();
                using var reader = new StreamReader(or.AsStream());
                var json = JsonObject.Parse(await reader.ReadToEndAsync());
                Layers.Clear();
                foreach (var ele in json["Layers"].GetArray())
                {
                    var layer = ele.GetObject();
                    switch (layer["LayerType"].GetString())
                    {
                        case "Ink":
                            {
                                using var ms = new MemoryStream();
                                var bytes = Convert.FromBase64String(layer["InkData"].GetString());
                                await ms.WriteAsync(bytes, 0, bytes.Length);
                                ms.Position = 0;
                                var inkCanvas = new WXInkCanvas(UndoManager);
                                await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(ms.AsRandomAccessStream());
                                Layers.Add(inkCanvas);
                            }
                            break;
                        case "Image":
                            {
                                Layers.Add(new WXImageCanvas() {
                                    Image = WXImage.FromBytes(Convert.FromBase64String(layer["ImageData"].GetString()))
                                });
                            }
                            break;
                    }
                }
                UndoManager.Clear();
                SelectedIndexProperty.Value = 0;
            }
            else if (e.Key == Windows.System.VirtualKey.V)
            {
                var img = await WXImage.FromClipboardAsync();
                if (img != null)
                {
                    Layers.Insert(SelectedIndexProperty.Value, new WXImageCanvas() { Image = img });
                }
            }
            else if (e.Key == Windows.System.VirtualKey.Z)
            {
                if (UndoManager.IsUndoableProperty.Value)
                    UndoManager.Undo();
            }
            else if (e.Key == Windows.System.VirtualKey.Y)
            {
                if (UndoManager.IsRedoableProperty.Value)
                    UndoManager.Redo();
            }
            else if (e.Key == Windows.System.VirtualKey.Up)
            {
                var idx = SelectedIndexProperty.Value;
                if (idx < Layers.Count - 1)
                {
                    Layers.Move(idx, idx + 1);
                }
            }
            else if (e.Key == Windows.System.VirtualKey.Down)
            {
                var idx = SelectedIndexProperty.Value;
                if (idx > 0)
                {
                    Layers.Move(idx, idx - 1);
                }
            }
            else if (e.Key == Windows.System.VirtualKey.Delete)
            {
                if (Layers.Count <= 0) return;
                var idx = SelectedIndexProperty.Value;
                if (idx >= 0)
                {
                    Layers.RemoveAt(idx);
                }
                if (idx < Layers.Count)
                {
                    SelectedIndexProperty.Value = idx;
                } else
                {
                    SelectedIndexProperty.Value = Layers.Count - 1;
                }
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