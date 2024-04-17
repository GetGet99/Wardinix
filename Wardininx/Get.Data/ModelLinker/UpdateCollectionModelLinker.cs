#nullable enable
namespace Get.Data.ModelLinker;

using System.Collections.Generic;
using System;
using System.Collections.Specialized;
using Get.Data.Collections.Update;
using Get.Data.Collections;

class UpdateCollectionModelLinker<T>(IUpdateReadOnlyCollection<T> source, IGDCollection<T> dest) : UpdateCollectionModelLinker<T, T>(source, dest)
{
    protected override T CreateFrom(T source) => source;
}

class UpdateCollectionModelLinkerDelegate<TSource, TDest>(IUpdateReadOnlyCollection<TSource> source, IGDCollection<TDest> dest, Func<TSource, TDest> createFrom) : UpdateCollectionModelLinker<TSource, TDest>(source, dest)
{
    protected override TDest CreateFrom(TSource source) => createFrom(source);
}
/// <summary>
/// Links the obserable collection with another collection, with a specific converter
/// </summary>
/// <typeparam name="TSourceCollection">The source collection type that implements <see cref="INotifyCollectionChanged"/></typeparam>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TDest">The destination type</typeparam>
abstract class UpdateCollectionModelLinker<TSource, TDest> : IDisposable
{
    public readonly IUpdateReadOnlyCollection<TSource> SourceUpdateCollection;
    public readonly IGDCollection<TDest> DestinationList;
    public event Action? UpdateCompleted;
    readonly LinkedList<TDest> hibernatedInstances = new();
    public UpdateCollectionModelLinker(IUpdateReadOnlyCollection<TSource> source, IGDCollection<TDest> dest)
    {
        SourceUpdateCollection = source;
        DestinationList = dest;
        source.ItemsChanged += Source_ItemsChanged;
    }

    private void Source_ItemsChanged(IEnumerable<IUpdateAction<TSource>> actions)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case ItemsAddedUpdateAction<TSource> added:
                    if (SourceUpdateCollection.Count - added.Items.Count != DestinationList.Count)
                        goto Reset;
                    for (int i = 0; i < added.Items.Count; i++)
                    {
                        DestinationList.Insert(added.StartingIndex + i, GetNewDest(added.Items[i]));
                        AfterItemAdded(added.Items[i], DestinationList[added.StartingIndex + i]);
                    }
                    break;
                case ItemsRemovedUpdateAction<TSource> removed:
                    if (SourceUpdateCollection.Count + removed.Items.Count != DestinationList.Count)
                        goto Reset;
                    for (int i = 0; i < removed.Items.Count; i++)
                    {
                        var oldItem = DestinationList[removed.StartingIndex + i];
                        DestinationList.RemoveAt(removed.StartingIndex + i);
                        MarkedRemovedFromDestination(oldItem);
                    }
                    break;
                case ItemsMovedUpdateAction<TSource> moved:
                    if (SourceUpdateCollection.Count != DestinationList.Count)
                        goto Reset;
                    (DestinationList[moved.OldIndex], DestinationList[moved.NewIndex]) =
                        (DestinationList[moved.NewIndex], DestinationList[moved.OldIndex]);
                    break;
                case ItemsReplacedUpdateAction<TSource> replaced:
                    if (SourceUpdateCollection.Count != DestinationList.Count)
                        goto Reset;
                    InplaceUpdate(replaced.Index);
                    break;
            }
        }
        goto End;
    Reset:
        PrivateResetAndReadd();
    End:
        OnUpdateCompleted();
        UpdateCompleted?.Invoke();
    }

    void InplaceUpdate(int idx)
    {
        var oldDestItem = DestinationList[idx];
        var newSrcItem = SourceUpdateCollection[idx];
        if (!TryInplaceUpdate(newSrcItem, DestinationList[idx]))
        {
            DestinationList[idx] = GetNewDest(newSrcItem);
            AfterItemAdded(newSrcItem, DestinationList[idx]);
            MarkedRemovedFromDestination(oldDestItem);
        }
    }
    void MarkedRemovedFromDestination(TDest item)
    {
        if (MarkedHibernation(item))
            hibernatedInstances.AddLast(item);
        else
            Recycle(item);
    }
    TDest GetNewDest(TSource src)
    {
    Start:
        if (hibernatedInstances.Count > 0)
        {
            var ret = ReturnFromHibernation(src, hibernatedInstances.First!.Value);
            if (ret is null)
            {
                hibernatedInstances.RemoveFirst();
                goto Start;
            }
            else if (ret.Value)
            {
                var val = hibernatedInstances.First.Value;
                hibernatedInstances.RemoveFirst();
                return val;
            }
            // we give up
        }
        // given up or no hibernate
        return CreateFrom(src);
    }
    protected virtual void AfterItemAdded(TSource source, TDest dest)
    {

    }
    protected virtual bool TryInplaceUpdate(TSource newItem, TDest currentItem)
    {
        return false;
    }
    protected virtual bool MarkedHibernation(TDest dest)
    {
        return false;
    }
    protected virtual bool? ReturnFromHibernation(TSource newItem, TDest dest)
    {
        return null;
    }
    protected virtual void OnUpdateCompleted()
    {

    }

    public void ResetAndReadd()
    {
        PrivateResetAndReadd();
        OnUpdateCompleted();
        UpdateCompleted?.Invoke();
    }
    void PrivateResetAndReadd()
    {
        int i = 0;
        while (i < DestinationList.Count && i < SourceUpdateCollection.Count)
        {
            InplaceUpdate(i++);
        }
        while (i < DestinationList.Count)
        {
            var ele = DestinationList[i];
            DestinationList.RemoveAt(i);
            MarkedRemovedFromDestination(ele);
        }
        while (i < SourceUpdateCollection.Count)
            DestinationList.Add(GetNewDest(SourceUpdateCollection[i++]));

        //DestinationList.Clear();
        //foreach (var item in SourceUpdateCollection)
        //{
        //    DestinationList.Add(GetNewDest(item));
        //}
    }

    protected abstract TDest CreateFrom(TSource source);
    protected virtual void Recycle(TDest dest) { }

    public void Dispose()
    {
        SourceUpdateCollection.ItemsChanged -= Source_ItemsChanged;
        foreach (var obj in hibernatedInstances) Recycle(obj);
        GC.SuppressFinalize(this);
    }
}