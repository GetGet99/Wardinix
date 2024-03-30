using Get.Data.ModelLinker;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Get.Data.Bindings;

public class CollectionBinder
{
    public static IDisposable Bind<TSrc, TOut>(ObservableCollection<TSrc> collection, IList<TOut> @out, DataTemplate<TSrc, TOut> dataTemplate)
        => Bind<ObservableCollection<TSrc>, TSrc, TOut>(collection, @out, dataTemplate);
    public static IDisposable Bind<TSrcCollection, TSrc, TOut>(TSrcCollection collection, IList<TOut> @out, DataTemplate<TSrc, TOut> dataTemplate) where TSrcCollection : INotifyCollectionChanged, IReadOnlyList<TSrc>
    {
        ObservableCollection<DataTemplateGeneratedValue<TSrc, TOut>> middleCollection = [];
        var a = new TemplateLinker<TSrcCollection, TSrc, TOut>(collection, middleCollection, dataTemplate);
        var b = new RefRemover<TSrc, TOut>(middleCollection, @out);
        a.ResetAndReadd();
        //b.ResetAndReadd();
        return new Disposable(() =>
        {
            a.Dispose();
            b.Dispose();
        });
    }
    public static IDisposable Bind<TSrc, TOut>(ObservableCollection<TSrc> collection, IList<TOut> @out, DataTemplate<(int Index, TSrc Value), TOut> dataTemplate)
        => Bind(new ObservableCollectionWithIndex<TSrc>(collection), @out, dataTemplate);
    public static IDisposable Bind<T>(ObservableCollection<T> collection, IList<T> @out)
    {
        var linker = new ObservableCollectionModelLinker<T>(collection, @out);
        linker.ResetAndReadd();
        return linker;
    }
    public static IDisposable Bind<TSrc, TOut>(ObservableCollection<TSrc> collection, IList<TOut> @out, Func<TSrc, TOut> convertFunc)
    {
        var linker = new ObservableCollectionModelLinkerDelegate<TSrc, TOut>(collection, @out, convertFunc);
        linker.ResetAndReadd();
        return linker;
    }
    public static IDisposable BindCast<T, TTarget>(ObservableCollection<T> collection, IList<TTarget> @out) where T : TTarget
    {
        var linker = new CastOCML<T, TTarget>(collection, @out);
        linker.ResetAndReadd();
        return linker;
    }
    class CastOCML<T, TTarget>(ObservableCollection<T> source, IList<TTarget> dest) : ObservableCollectionModelLinker<T, TTarget>(source, dest) where T : TTarget
    {
        protected override TTarget CreateFrom(T source) => source;
    }
}
readonly struct Disposable(Action OnDispose) : IDisposable
{
    public void Dispose() => OnDispose();
}
class TemplateLinker<TSourceCollection, TSource, TDest>(TSourceCollection source, ObservableCollection<DataTemplateGeneratedValue<TSource, TDest>> dest, DataTemplate<TSource, TDest> dataTemplate) : ObservableCollectionModelLinker<TSourceCollection, TSource, DataTemplateGeneratedValue<TSource, TDest>>(source, dest) where TSourceCollection : IReadOnlyList<TSource>, INotifyCollectionChanged
{
    protected override DataTemplateGeneratedValue<TSource, TDest> CreateFrom(TSource source)
    {
        return dataTemplate.Generate(source);
    }
    protected override void Recycle(DataTemplateGeneratedValue<TSource, TDest> dest)
    {
        base.Recycle(dest);
        dest.Recycle();
    }
    protected override bool MarkedHibernation(DataTemplateGeneratedValue<TSource, TDest> dest)
    {
        return true;
    }
    protected override bool? ReturnFromHibernation(TSource newItem, DataTemplateGeneratedValue<TSource, TDest> dest)
    {
        dest.Binding = new ValueBinding<TSource>(newItem);
        return true;
    }
    protected override bool TryInplaceUpdate(TSource newItem, DataTemplateGeneratedValue<TSource, TDest> currentItem)
    {
        currentItem.Binding = new ValueBinding<TSource>(newItem);
        return true;
    }
}
class ObservableCollectionWithIndex<T> : INotifyCollectionChanged, IReadOnlyList<(int Index, T Value)>
{
    readonly ObservableCollection<T> Original;
    public ObservableCollectionWithIndex(ObservableCollection<T> original)
    {
        Original = original;
        original.CollectionChanged += Original_CollectionChanged;
    }

    private void Original_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        for (int i = 0; i < e.NewItems.Count; i++)
        {
            e.NewItems[i] = (i, e.NewItems[i]);
        }
        for (int i = 0; i < e.NewItems.Count; i++)
        {
            e.OldItems[i] = (i, e.OldItems[i]);
        }
        CollectionChanged?.Invoke(this, e);
    }

    public (int Index, T Value) this[int index] => (index, Original[index]);

    public int Count => Original.Count;

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public IEnumerator<(int Index, T Value)> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
//class TemplateLinkerWithIndex<TSource, TDest>(ObservableCollection<TSource> source, ObservableCollection<DataTemplateGeneratedValue<(TSource Value, int Index), TDest>> dest, DataTemplate<TSource, TDest> dataTemplate) : ObservableCollectionModelLinker<TSource, DataTemplateGeneratedValue<(TSource Value, int Index), TDest>>(source, dest)
//{
//    protected override DataTemplateGeneratedValue<(TSource Value, int Index), TDest> CreateFrom(TSource source)
//    {
//        return dataTemplate.Generate(source);
//    }
//    protected override void Recycle(DataTemplateGeneratedValue<(TSource Value, int Index), TDest> dest)
//    {
//        base.Recycle(dest);
//        dest.Recycle();
//    }
//    protected override bool MarkedHibernation(DataTemplateGeneratedValue<(TSource Value, int Index), TDest> dest)
//    {
//        return true;
//    }
//    protected override bool? ReturnFromHibernation(TSource newItem, DataTemplateGeneratedValue<(TSource Value, int Index), TDest> dest)
//    {
//        dest.Binding = new ValueBinding<TSource>(newItem);
//        return true;
//    }
//    protected override bool TryInplaceUpdate(TSource newItem, DataTemplateGeneratedValue<(TSource Value, int Index), TDest> currentItem)
//    {
//        currentItem.Binding = new ValueBinding<TSource>(newItem);
//        return true;
//    }
//}
class RefRemover<TSrc, TOut>(ObservableCollection<DataTemplateGeneratedValue<TSrc, TOut>> source, IList<TOut> dest) : ObservableCollectionModelLinker<DataTemplateGeneratedValue<TSrc, TOut>, TOut>(source, dest)
{
    protected override TOut CreateFrom(DataTemplateGeneratedValue<TSrc, TOut> source)
        => source.GeneratedValue;
}