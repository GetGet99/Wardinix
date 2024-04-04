#nullable enable
namespace Get.Data.ModelLinker;

using System.Collections.Generic;
using System;
using System.Collections.Specialized;
using Get.Data.Collections;

class UpdateCollectionModelLinker<T>(IReadOnlyUpdateCollection<T> source, IList<T> dest) : UpdateCollectionModelLinker<T, T>(source, dest)
{
    protected override T CreateFrom(T source) => source;
}

class UpdateCollectionModelLinkerDelegate<TSource, TDest>(IReadOnlyUpdateCollection<TSource> source, IList<TDest> dest, Func<TSource, TDest> createFrom) : UpdateCollectionModelLinker<TSource, TDest>(source, dest)
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
    public readonly IReadOnlyUpdateCollection<TSource> SourceUpdateCollection;
    public readonly IList<TDest> DestinationList;
    public event Action? UpdateCompleted;
    readonly LinkedList<TDest> hibernatedInstances = new();
    public UpdateCollectionModelLinker(IReadOnlyUpdateCollection<TSource> source, IList<TDest> dest)
    {
        SourceUpdateCollection = source;
        DestinationList = dest;
        source.ItemsAdded += Source_ItemsAdded;
        source.ItemsRemoved += Source_ItemsRemoved;
        source.ItemsMoved += Source_ItemsMoved;
        source.ItemsReplaced += Source_ItemsReplaced;
    }

    private void Source_ItemsAdded(int startingIndex, IReadOnlyList<TSource> item)
    {
        if (SourceUpdateCollection.Count - item.Count != DestinationList.Count)
        {
            PrivateResetAndReadd();
            goto End;
        }
        for (int i = 0; i < item.Count; i++)
        {
            DestinationList.Insert(startingIndex + i, GetNewDest(item[i]));
            AfterItemAdded(item[i], DestinationList[startingIndex + i]);
        }
    End:
        OnUpdateCompleted();
        UpdateCompleted?.Invoke();
    }
    private void Source_ItemsRemoved(int startingIndex, IReadOnlyList<TSource> item)
    {
        if (SourceUpdateCollection.Count != DestinationList.Count - 1)
        {
            PrivateResetAndReadd();
            goto End;
        }
        for (int i = 0; i < item.Count; i++)
        {
            var oldItem = DestinationList[startingIndex + i];
            DestinationList.RemoveAt(startingIndex + i);
            MarkedRemovedFromDestination(oldItem);
        }
    End:
        OnUpdateCompleted();
        UpdateCompleted?.Invoke();
    }
    private void Source_ItemsMoved(int oldIndex, int newIndex, TSource oldIndexItem, TSource newIndexItem)
    {
        if (SourceUpdateCollection.Count != DestinationList.Count)
        {
            PrivateResetAndReadd();
            goto End;
        }
        (DestinationList[oldIndex], DestinationList[newIndex]) = (DestinationList[newIndex], DestinationList[oldIndex]);
    End:
        OnUpdateCompleted();
        UpdateCompleted?.Invoke();
    }

    private void Source_ItemsReplaced(int index, TSource oldItem, TSource newItem)
    {
        if (SourceUpdateCollection.Count != DestinationList.Count)
        {
            PrivateResetAndReadd();
            goto End;
        }
        InplaceUpdate(index);
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
            MarkedRemovedFromDestination(DestinationList[i++]);
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
        SourceUpdateCollection.ItemsAdded -= Source_ItemsAdded;
        SourceUpdateCollection.ItemsRemoved -= Source_ItemsRemoved;
        SourceUpdateCollection.ItemsMoved -= Source_ItemsMoved;
        SourceUpdateCollection.ItemsReplaced -= Source_ItemsReplaced;
        foreach (var obj in hibernatedInstances) Recycle(obj);
        GC.SuppressFinalize(this);
    }
}