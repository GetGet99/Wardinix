using Get.Data.Collections;
using Get.Data.Collections.Update;
using Get.Data.Collections.Linq;
using Wardininx.Classes;
using Wardininx.UndoRedos;
using Windows.Data.Json;
using Wardininx.Core.Layers;

namespace Wardininx.Core;

/// <summary>
/// The core functionality of a single document.
/// </summary>
public class DocumentCore : IRawDocumentEdit
{
    public ReadOnlyProperty<IUpdateReadOnlyCollection<LayerCore>> LayersProperty { get; }
    public IUpdateReadOnlyCollection<LayerCore> Layers => LayersProperty.Value;
    IUpdateCollection<LayerCore> IRawDocumentEdit.Layers => _layers;
    readonly UpdateCollection<LayerCore> _layers = new();
    public DocumentCore()
    {
        LayersProperty = new(_layers);
    }
    public async static Task<DocumentCore> CreateAsync(JsonObject json)
    {
        DocumentCore toReturn = new();
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
                        var inkCanvas = new InkLayerCore();
                        await inkCanvas.InkControllerCore.InkPresenter.StrokeContainer.LoadAsync(ms.AsRandomAccessStream());
                        toReturn._layers.Add(inkCanvas);
                    }
                    break;
                case "Image":
                    {
                        toReturn._layers.Add(new ImageLayerCore()
                        {
                            Image = WXImage.FromBytes(Convert.FromBase64String(layer["ImageData"].GetString())),
                            //Offset = CanvasScrollOffsetProperty.Value,
                            //CanvasScale = CanvasScaleProperty.Value
                        });
                    }
                    break;
            }
        }
        return toReturn;
    }
    public async Task<JsonObject> SaveAsync()
    {
        var jsonArr = new JsonArray();
        foreach (var layer in Layers.AsEnumerable())
        {
            switch (layer)
            {
                case InkLayerCore inkCanvas:
                    {
                        using var ms = new MemoryStream();
                        await inkCanvas.InkControllerCore.InkPresenter.StrokeContainer.SaveAsync(ms.AsRandomAccessStream());
                        jsonArr.Add(new JsonObject
                                    {
                                        { "LayerType", JsonValue.CreateStringValue("Ink") },
                                        { "InkData", JsonValue.CreateStringValue(Convert.ToBase64String(ms.ToArray())) }
                                    });
                    }
                    break;
                case ImageLayerCore imageCanvas:
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
        return new JsonObject()
        {
            { "Type", JsonValue.CreateStringValue("LayerContainer") },
            { "Layers", jsonArr }
        };
    }
}
public interface IRawDocumentEdit
{
    IUpdateCollection<LayerCore> Layers { get; }
}