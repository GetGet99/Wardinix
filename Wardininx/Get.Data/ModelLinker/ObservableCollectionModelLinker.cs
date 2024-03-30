namespace Get.Data.ModelLinker;

using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;


class ObservableCollectionModelLinker<T>(ObservableCollection<T> source, IList<T> dest) : ObservableCollectionModelLinker<T, T>(source, dest)
{
    protected override T CreateFrom(T source) => source;
}

abstract class ObservableCollectionModelLinker<TSource, TDest>(ObservableCollection<TSource> source, IList<TDest> dest) : ObservableCollectionModelLinker<ObservableCollection<TSource>, TSource, TDest>(source, dest)
{
}
class ObservableCollectionModelLinkerDelegate<TSource, TDest>(ObservableCollection<TSource> source, IList<TDest> dest, Func<TSource, TDest> createFrom) : ObservableCollectionModelLinker<TSource, TDest>(source, dest)
{
    protected override TDest CreateFrom(TSource source) => createFrom(source);
}
/// <summary>
/// Links the obserable collection with another collection, with a specific converter
/// </summary>
/// <typeparam name="TSourceCollection">The source collection type that implements <see cref="INotifyCollectionChanged"/></typeparam>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TDest">The destination type</typeparam>
abstract class ObservableCollectionModelLinker<TSourceCollection, TSource, TDest> : IDisposable where TSourceCollection : INotifyCollectionChanged, IReadOnlyList<TSource>
{
    public readonly TSourceCollection SourceObservableCollection;
    public readonly IList<TDest> DestinationList;
    public event Action? UpdateCompleted;
    readonly LinkedList<TDest> hibernatedInstances = new();
    public ObservableCollectionModelLinker(TSourceCollection source, IList<TDest> dest)
    {
        this.SourceObservableCollection = source;
        this.DestinationList = dest;
        source.CollectionChanged += CollectionChanged;
    }

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var newindex = e.NewStartingIndex;
        var oldindex = e.OldStartingIndex;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (SourceObservableCollection.Count - 1 != DestinationList.Count)
                {
                    PrivateResetAndReadd();
                    break;
                }
                var lp = SourceObservableCollection[newindex];
                DestinationList.Insert(newindex, GetNewDest(lp));
                AfterItemAdded(lp, DestinationList[newindex]);
                break;
            case NotifyCollectionChangedAction.Remove:
                {
                    if (SourceObservableCollection.Count != DestinationList.Count - 1)
                    {
                        PrivateResetAndReadd();
                        break;
                    }
                    var oldItem = DestinationList[oldindex];
                    DestinationList.RemoveAt(oldindex);
                    MarkedRemovedFromDestination(oldItem);
                }
                break;
            case NotifyCollectionChangedAction.Move:
                if (SourceObservableCollection.Count != DestinationList.Count)
                {
                    PrivateResetAndReadd();
                    break;
                }
                (DestinationList[oldindex], DestinationList[newindex]) = (DestinationList[newindex], DestinationList[oldindex]);
                break;
            case NotifyCollectionChangedAction.Replace:
                {
                    if (SourceObservableCollection.Count != DestinationList.Count)
                    {
                        PrivateResetAndReadd();
                        break;
                    }
                    InplaceUpdate(oldindex);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (var item in DestinationList.ToArray()) MarkedRemovedFromDestination(item);
                DestinationList.Clear();
                break;
        }
        OnUpdateCompleted();
        UpdateCompleted?.Invoke();
    }
    void InplaceUpdate(int idx)
    {
        var oldDestItem = DestinationList[idx];
        var newSrcItem = SourceObservableCollection[idx];
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
        while (i < DestinationList.Count && i < SourceObservableCollection.Count)
        {
            InplaceUpdate(i++);
        }
        while (i < DestinationList.Count)
            MarkedRemovedFromDestination(DestinationList[i++]);
        while (i < SourceObservableCollection.Count)
            DestinationList.Add(GetNewDest(SourceObservableCollection[i++]));

        //DestinationList.Clear();
        //foreach (var item in SourceObservableCollection)
        //{
        //    DestinationList.Add(GetNewDest(item));
        //}
    }

    protected abstract TDest CreateFrom(TSource source);
    protected virtual void Recycle(TDest dest) { }

    public void Dispose()
    {
        SourceObservableCollection.CollectionChanged -= CollectionChanged;
        foreach (var obj in hibernatedInstances) Recycle(obj);
        GC.SuppressFinalize(this);
    }
}