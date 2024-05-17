#nullable enable
using Get.Data.Collections;
using Get.Data.Collections.Linq;
using Get.Data.Collections.Update;
using Wardininx.Controls.Canvas;

namespace Wardininx.API;

partial class LayersCollection : CollectionUpdateEvent<ILayer>
{
    public ILayer this[int index] { get => layerCores[index].GetEditingSession(document); set => layerCores[index] = value.Core; }
    LayerCore IGDFixedSizeCollection<LayerCore>.this[int index] { get => layerCores[index]; set => layerCores[index] = value; }
    LayerCore IGDReadOnlyCollection<LayerCore>.this[int index] => layerCores[index];
    UpdateCollectionItemsChanged<LayerCore>? _ItemsChangedLC;

    event UpdateCollectionItemsChanged<LayerCore> ICollectionUpdateEvent<LayerCore>.ItemsChanged
    {
        add
        {
            if (_ItemsChangedLC == null) RegisterItemsChangedEvent();
            _ItemsChangedLC += value;
        }
        remove
        {
            _ItemsChangedLC -= value;
            if (_ItemsChangedLC == null) UnregisterItemsChangedEvent();
        }
    }

    public int Count => layerCores.Count;


    public void Clear()
    {
        var layersCache = layerCores.EvalList();
        layerCores.Clear();
        document.UndoManager.DoAndAddAction(new(
            ActionName: $"Layers.Clear()",
            Redo: delegate {
                layerCores.Clear();
                document.SelectedIndex = -1;
            },
            Undo: delegate
            {
                layerCores.AddRange(layersCache);
                if (layerCores.Count > 0)
                    document.SelectedIndex = 0;
            },
            Cleanup: delegate { }
        ));
    }

    public void Insert(int index, ILayer item) => Insert(index, item.Core);
    public void Insert(int index, LayerCore item)
    {
        document.UndoManager.DoAndAddAction(new(
            ActionName: $"Layers.Insert({index}, {item})",
            Redo: delegate
            {
                layerCores.Insert(index, item);
                if (document.SelectedIndex >= index)
                    document.SelectedIndex = index + 1;
            },
            Undo: delegate
            {
                layerCores.RemoveAt(index);
                if (document.SelectedIndex >= index)
                    document.SelectedIndex = index - 1;
            },
            Cleanup: delegate { }
        ));
    }

    public void Move(int index1, int index2)
    {
        document.UndoManager.DoAndAddAction(new(
            ActionName: $"Layers.Move({index1}, {index2})",
            Redo: delegate
            {
                layerCores.Move(index1, index2);
                if (document.SelectedIndex == index1)
                    document.SelectedIndex = index2;
                else if (document.SelectedIndex == index2)
                    document.SelectedIndex = index1;
            },
            Undo: delegate
            {
                layerCores.Move(index2, index1);
                if (document.SelectedIndex == index1)
                    document.SelectedIndex = index2;
                else if (document.SelectedIndex == index2)
                    document.SelectedIndex = index1;
            },
            Cleanup: delegate { }
        ));
    }

    public void RemoveAt(int index)
    {
        var item = layerCores[index];
        document.UndoManager.DoAndAddAction(new(
            ActionName: $"Layers.RemoveAt({index})",
            Redo: delegate
            {
                layerCores.RemoveAt(index);
                if (document.SelectedIndex >= index)
                    document.SelectedIndex = index - 1;
            },
            Undo: delegate
            {
                layerCores.Insert(index, item);
                if (document.SelectedIndex >= index)
                    document.SelectedIndex = index + 1;
            },
            Cleanup: delegate { }
        ));
    }

    protected override void RegisterItemsChangedEvent()
    {
        layerCores.ItemsChanged += LayerCoresItemsChanged;
    }


    protected override void UnregisterItemsChangedEvent()
    {
        layerCores.ItemsChanged -= LayerCoresItemsChanged;
    }
    private void LayerCoresItemsChanged(IEnumerable<IUpdateAction<LayerCore>> actions)
    {
        InvokeItemsChanged(LayersItemsProcessor(actions));
        _ItemsChangedLC?.Invoke(actions);
    }
    IEnumerable<IUpdateAction<ILayer>> LayersItemsProcessor(IEnumerable<IUpdateAction<LayerCore>> actions)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case ItemsAddedUpdateAction<LayerCore> a:
                    yield return new ItemsAddedUpdateAction<ILayer>(
                        a.StartingIndex,
                        a.Items.Select(x => x.GetEditingSession(document)),
                        a.OldCollectionCount
                    );
                    break;
                case ItemsRemovedUpdateAction<LayerCore> rem:
                    yield return new ItemsRemovedUpdateAction<ILayer>(
                        rem.StartingIndex,
                        rem.Items.Select(x => x.GetEditingSession(document)),
                        rem.OldCollectionCount
                    );
                    break;
                case ItemsReplacedUpdateAction<LayerCore> rep:
                    yield return new ItemsReplacedUpdateAction<ILayer>(
                        rep.Index,
                        rep.OldItem.GetEditingSession(document),
                        rep.NewItem.GetEditingSession(document)
                    );
                    break;
                case ItemsMovedUpdateAction<LayerCore> m:
                    yield return new ItemsMovedUpdateAction<ILayer>(
                        m.OldIndex,
                        m.NewIndex,
                        m.OldIndexItem.GetEditingSession(document),
                        m.NewIndexItem.GetEditingSession(document)
                    );
                    break;
            }
        }
    }
}