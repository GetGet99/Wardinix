using Get.Data.Collections.Update;
using Get.Data.Collections.Linq;
namespace Get.Data.Collections;

abstract class SelectUpdateBase<TSrc, TDest>(IUpdateReadOnlyCollection<TSrc> src, ForwardConverter<TSrc, TDest> forward) : CollectionUpdateEvent<TDest>
{
    protected sealed override void RegisterItemsChangedEvent()
        => src.ItemsChanged += Internal_ItemsChangedHandler;

    protected sealed override void UnregisterItemsChangedEvent()
        => src.ItemsChanged -= Internal_ItemsChangedHandler;


    private void Internal_ItemsChangedHandler(IEnumerable<IUpdateAction<TSrc>> actions)
        => InvokeItemsChanged(actions.Select(x => (IUpdateAction<TDest>)(
        x switch
        {
            ItemsAddedUpdateAction<TSrc> x1 =>
                new ItemsAddedUpdateAction<TDest>(x1.StartingIndex, x1.Items.Select(forward)),
            ItemsRemovedUpdateAction<TSrc> x1 =>
                new ItemsRemovedUpdateAction<TDest>(x1.StartingIndex, x1.Items.Select(forward)),
            ItemsMovedUpdateAction<TSrc> x1 =>
                new ItemsMovedUpdateAction<TDest>(
                    x1.OldIndex, x1.NewIndex, forward(x1.OldIndexItem), forward(x1.NewIndexItem)
                ),
            ItemsReplacedUpdateAction<TSrc> x1 =>
                new ItemsReplacedUpdateAction<TDest>(
                    x1.Index, forward(x1.OldItem), forward(x1.NewItem)
                ),
            _ => throw new InvalidCastException()
        })));
}