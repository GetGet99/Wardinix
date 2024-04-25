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
using Windows.System;
using Windows.UI.Core;
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
        toolbar.LayerToolbar.SelectedIndexProperty.Bind(SelectedIndexProperty, Get.Data.Bindings.BindingModes.TwoWay);
        bool IsKeyDown(VirtualKey key)
            => CoreWindow.GetForCurrentThread().GetAsyncKeyState(key) != CoreVirtualKeyStates.None;
        KeyDown += async (o, e) =>
        {
            if (IsKeyDown(VirtualKey.Control))
            {
                switch (e.Key)
                {
                    case VirtualKey.S:
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
                            Focus(FocusState.Programmatic);
                        }
                        return;
                    case VirtualKey.O:
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
                                            Layers.Add(new WXImageCanvas()
                                            {
                                                Image = WXImage.FromBytes(Convert.FromBase64String(layer["ImageData"].GetString()))
                                            });
                                        }
                                        break;
                                }
                            }
                            UndoManager.Clear();
                            SelectedIndexProperty.Value = 0;
                            Focus(FocusState.Programmatic);
                        }
                        return;
                    case VirtualKey.V:
                        {
                            var img = await WXImage.FromClipboardAsync();
                            if (img != null)
                            {
                                var idx = SelectedIndexProperty.Value;
                                var layer = new WXImageCanvas() { Image = img };
                                UndoManager.DoAndAddAction(new UndoableAction<(int Index, WXCanvasControl Layer)>("Delete Layer", (idx, layer),
                                    x =>
                                    {
                                        Layers.RemoveAt(idx);
                                        if (x.Index < Layers.Count)
                                        {
                                            SelectedIndexProperty.Value = idx;
                                        }
                                        else
                                        {
                                            SelectedIndexProperty.Value = Layers.Count - 1;
                                        }
                                    },
                                    x =>
                                    {
                                        Layers.Insert(x.Index, x.Layer);
                                        SelectedIndexProperty.Value = x.Index;
                                    },
                                    delegate { }
                                ));
                            }
                        }
                        return;
                    case VirtualKey.Z:
                        if (IsKeyDown(VirtualKey.Shift)) goto case VirtualKey.Y;
                        if (UndoManager.IsUndoableProperty.Value)
                            UndoManager.Undo();
                        return;
                    case VirtualKey.Y:
                        if (UndoManager.IsRedoableProperty.Value)
                            UndoManager.Redo();
                        return;
                }
            }
            if (IsKeyDown(VirtualKey.Shift))
            {
                if (e.Key == VirtualKey.Up)
                {
                    var idx = SelectedIndexProperty.Value;
                    if (idx < Layers.Count - 1)
                    {
                        UndoManager.DoAndAddAction(new UndoableAction<int>("Move Layer Up", idx,
                            idx =>
                            {
                                Layers.Move(idx + 1, idx);
                                SelectedIndexProperty.Value = idx;
                            },
                            idx =>
                            {
                                Layers.Move(idx, idx + 1);
                                SelectedIndexProperty.Value = idx + 1;
                            },
                            delegate { }
                        ));

                    }
                    return;
                }
                else if (e.Key == VirtualKey.Down)
                {
                    var idx = SelectedIndexProperty.Value;
                    if (idx > 0)
                    {
                        UndoManager.DoAndAddAction(new UndoableAction<int>("Move Layer Down", idx,
                            idx =>
                            {
                                Layers.Move(idx - 1, idx);
                                SelectedIndexProperty.Value = idx;
                            },
                            idx =>
                            {
                                Layers.Move(idx, idx - 1);
                                SelectedIndexProperty.Value = idx - 1;
                            },
                            delegate { }
                        ));
                    }
                    return;
                }
            }
            // no modifier keys
            {
                switch (e.Key)
                {
                    case VirtualKey.W:
                    case VirtualKey.Up:
                        {
                            var idx = SelectedIndexProperty.Value;
                            if (idx < Layers.Count - 1)
                            {
                                SelectedIndexProperty.Value = idx + 1;
                            }
                            return;
                        }
                    case VirtualKey.S:
                    case VirtualKey.Down:
                        {
                            var idx = SelectedIndexProperty.Value;
                            if (idx > 0)
                            {
                                SelectedIndexProperty.Value = idx - 1;
                            }
                            return;
                        }
                    case VirtualKey.Delete:
                        {
                            if (Layers.Count <= 0) return;
                            var idx = SelectedIndexProperty.Value;
                            if (idx < 0) return;
                            var layer = Layers[idx];
                            UndoManager.DoAndAddAction(new UndoableAction<(int Index, WXCanvasControl Layer)>("Delete Layer", (idx, layer),
                                x =>
                                {
                                    Layers.Insert(x.Index, x.Layer);
                                    SelectedIndexProperty.Value = x.Index;
                                },
                                x =>
                                {
                                    Layers.RemoveAt(idx);
                                    if (x.Index < Layers.Count)
                                    {
                                        SelectedIndexProperty.Value = idx;
                                    }
                                    else
                                    {
                                        SelectedIndexProperty.Value = Layers.Count - 1;
                                    }
                                },
                                delegate { }
                            ));
                            return;
                        }
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