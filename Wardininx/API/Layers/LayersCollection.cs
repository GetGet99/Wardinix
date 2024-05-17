using Get.Data.Collections;
using Get.Data.Collections.Linq;
using Get.Data.Collections.Update;
using Wardininx.Controls.Canvas;

namespace Wardininx.API;

public partial class LayersCollection(Document document, IUpdateCollection<LayerCore> layerCores) : IUpdateCollection<ILayer>, IUpdateCollection<LayerCore>
{
    public void AddSelect(ILayer item) => InsertSelect(Count, item);
    
    public void InsertSelect(int index, ILayer item)
    {
        var oldSelection = document.SelectedIndex;
        document.UndoManager.DoAndAddAction(new(
            ActionName: $"Layers.InsertSelect({index}, {item})",
            Redo: delegate
            {
                layerCores.Insert(index, item.Core);
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