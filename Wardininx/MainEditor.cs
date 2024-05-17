using Get.Data.Collections;
using Get.Data.Collections.Linq;
using Wardininx.API;
using Wardininx.Classes;
using Wardininx.Controls.Canvas;
using Wardininx.Controls.Toolbars;
using Wardininx.Core;
using Wardininx.UndoRedos;
using Windows.Data.Json;
using Windows.Foundation;
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
        InkLayerCore CreateInkCanvas() => new(UndoManager);
        var MicaBrush = new BlurredWallpaperMaterials.BackdropMaterial
        {
            Kind = (int)BlurredWallpaperMaterials.BackdropKind.Base,
            Theme = ActualTheme
        };
        ActualThemeChanged += (_1, _2) => MicaBrush.Theme = ActualTheme;
        Document document = new(new());
        document.Layers.Add(CreateInkCanvas().GetEditingSession(document));
        var Layers = document.Layers;
        var SelectedIndexProperty = document.SelectedIndexProperty;
        var CanvasViewOffsetProperty = document.CanvasViewOffsetProperty;
        Property<float> CanvasScaleProperty = new(default);
        Content = new Grid
        {
            Background = MicaBrush,
            Children =
            {
                new WXCanvasController { Layers = Layers }
                .WithTwoWayUpdateSourceImmedieteBinding(
                    new()
                    {
                        { WXCanvasController.CanvasScrollOffsetPropertyDefinition, CanvasViewOffsetProperty },
                        { WXCanvasController.CanvasScalePropertyDefinition, CanvasScaleProperty }
                    }
                )
                .UnsafeGetElement<UIElement>(),
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
                            
                            using var stream = await file.OpenStreamForWriteAsync();
                            stream.Position = 0;
                            using StreamWriter writer = new(stream);
                            await writer.WriteAsync(
                                (document as IEditingSession<DocumentCore>).Core.ToString()
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
                            var doc = await DocumentCore.CreateAsync(json);
                            Layers.AddRange(doc.Layers);
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
                                var scale = 1 / CanvasScaleProperty.Value;
                                var layer = new ImageLayerCore() {
                                    Image = img,
                                    Offset = CanvasViewOffsetProperty.Value,
                                    CanvasScale = new(scale, scale, scale)
                                };
                                Layers.Insert(idx, layer);
                            }
                        }
                        return;
                    case VirtualKey.Z:
                        if (IsKeyDown(VirtualKey.Shift)) goto case VirtualKey.Y;
                        if (UndoManager.IsUndoable) UndoManager.Undo();
                        return;
                    case VirtualKey.Y:
                        if (UndoManager.IsRedoable) UndoManager.Redo();
                        return;
                }
            }
            if (IsKeyDown(VirtualKey.Shift))
            {
                if (e.Key == VirtualKey.Up)
                {
                    var idx = SelectedIndexProperty.Value;
                    if (idx < Layers.Count - 2)
                    {
                        Layers.Move(idx + 1, idx);
                    }
                    return;
                }
                else if (e.Key == VirtualKey.Down)
                {
                    var idx = SelectedIndexProperty.Value;
                    if (idx > 0)
                    {
                        Layers.Move(idx - 1, idx);
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
                            Layers.RemoveAt(idx);
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