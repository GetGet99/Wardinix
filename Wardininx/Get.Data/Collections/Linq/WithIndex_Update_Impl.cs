using Get.Data.Collections.Linq;
using Get.Data.Collections;
using Get.Data.Collections.Update;

namespace Get.Data.Collections;
abstract class WithIndexUpdateBase<T>(IUpdateReadOnlyCollection<T> src) : CollectionUpdateEvent<IndexItem<T>>
{

    protected sealed override void RegisterItemsChangedEvent() => src.ItemsChanged += Src_ItemsChanged;
    protected sealed override void UnregisterItemsChangedEvent() => src.ItemsChanged -= Src_ItemsChanged;

    private void Src_ItemsChanged(IEnumerable<IUpdateAction<T>> actions)
    {
        InvokeItemsChanged(ProcessActions(actions));
    }
    IEnumerable<IUpdateAction<IndexItem<T>>> ProcessActions(IEnumerable<IUpdateAction<T>> actions)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case ItemsAddedUpdateAction<T> added:
                    {
                        (int startingIndex, IGDReadOnlyCollection<T> item) = (added.StartingIndex, added.Items);
                        int itemsAdded = item.Count;
                        yield return new ItemsAddedUpdateAction<IndexItem<T>>(
                            startingIndex, item.WithIndex().Select(x => new(x.Index + startingIndex, x.Value))
                        );
                        // Sends update to everything else above that
                        for (int i = startingIndex + item.Count - 1; i < src.Count; i++)
                        {
                            yield return new ItemsReplacedUpdateAction<IndexItem<T>>(i, new(i - itemsAdded, src[i]), new(i, src[i]));
                        }
                    }
                    break;
                case ItemsRemovedUpdateAction<T> removed:
                    {
                        (int startingIndex, IGDReadOnlyCollection<T> item) = (removed.StartingIndex, removed.Items);
                        int itemsRemoved = item.Count;
                        yield return new ItemsRemovedUpdateAction<IndexItem<T>>(
                            startingIndex, item.WithIndex().Select(x => new(x.Index + startingIndex, x.Value))
                        );
                        // Sends update to everything else above that
                        for (int i = startingIndex + item.Count - 1; i < src.Count; i++)
                        {
                            yield return new ItemsReplacedUpdateAction<IndexItem<T>>(i, new(i + itemsRemoved, src[i]), new(i, src[i]));
                        }
                    }
                    break;
                    case ItemsMovedUpdateAction<T> moved:
                    {
                        yield return new ItemsMovedUpdateAction<IndexItem<T>>(
                            moved.OldIndex,
                            moved.NewIndex,
                            new(moved.OldIndex, moved.OldIndexItem),
                            new(moved.NewIndex, moved.NewIndexItem)
                        );
                    }
                    break;
                case ItemsReplacedUpdateAction<T> replaced:
                    {
                        yield return new ItemsReplacedUpdateAction<IndexItem<T>>(
                            replaced.Index,
                            new(replaced.Index, replaced.OldItem),
                            new(replaced.Index, replaced.NewItem)
                        );
                    }
                    break;
            }
        }
    }
}