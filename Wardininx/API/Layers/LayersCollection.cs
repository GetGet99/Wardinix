using Get.Data.Collections.Update;
using Wardininx.Core.Layers;

namespace Wardininx.API;

public partial class LayersCollection(Document document, IUpdateCollection<LayerCore> layerCores) : IUpdateCollection<ILayer>, IUpdateCollection<LayerCore>
{
    public void AddSelect(ILayer item) => InsertSelect(Count, item);
    public void AddSelect(LayerCore item) => InsertSelect(Count, item);

    public void InsertSelect(int index, ILayer item)
        => InsertSelect(index, item.Core);
    public void InsertSelect(int index, LayerCore item)
    {
        var oldSelection = document.SelectedIndex;
        document.UndoManager.DoAndAddAction(new(
            ActionName: $"Layers.InsertSelect({index}, {item})",
            Redo: delegate
            {
                layerCores.Insert(index, item);
                document.SelectedIndex = index;
            },
            Undo: delegate
            {
                layerCores.RemoveAt(index);
                document.SelectedIndex = oldSelection;
            },
            Cleanup: delegate { }
        ));
    }
    public void MoveSelect(int index1, int index2)
    {
        var oldSelection = document.SelectedIndex;
        document.UndoManager.DoAndAddAction(new(
            ActionName: $"Layers.MoveSelect({index1}, {index2})",
            Redo: delegate
            {
                layerCores.Move(index1, index2);
                document.SelectedIndex = index2;
            },
            Undo: delegate
            {
                layerCores.Move(index2, index1);
                document.SelectedIndex = oldSelection;
            },
            Cleanup: delegate { }
        ));
    }
}