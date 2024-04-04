using Get.Data.Bindings;
using Get.Data.ModelLinker;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
namespace Get.Data.Collections;
public static class CollectionBinderExtension
{
    public static IDisposable Bind<TSrc, TOut>(this IUpdateCollection<TSrc> collection, IList<TOut> @out, DataTemplate<TSrc, TOut> dataTemplate)
        => collection.AsReadOnly().Bind(@out, dataTemplate);
    public static IDisposable Bind<TSrc, TOut>(this IReadOnlyUpdateCollection<TSrc> collection, IList<TOut> @out, DataTemplate<TSrc, TOut> dataTemplate)
    {
        UpdateCollection<DataTemplateGeneratedValue<TSrc, TOut>> middleCollection = [];
        var a = new TemplateLinker<TSrc, TOut>(collection, middleCollection, dataTemplate);
        var b = new RefRemover<TSrc, TOut>(middleCollection.AsReadOnly(), @out);
        a.ResetAndReadd();
        //b.ResetAndReadd();
        return new Disposable(() =>
        {
            a.Dispose();
            b.Dispose();
        });
    }
    public static IDisposable Bind<T>(this IUpdateCollection<T> collection, IList<T> @out)
        => collection.AsReadOnly().Bind(@out);
    public static IDisposable Bind<T>(this IReadOnlyUpdateCollection<T> collection, IList<T> @out)
    {
        var linker = new UpdateCollectionModelLinker<T>(collection, @out);
        linker.ResetAndReadd();
        return linker;
    }
}

class TemplateLinker<TSource, TDest>(IReadOnlyUpdateCollection<TSource> source, UpdateCollection<DataTemplateGeneratedValue<TSource, TDest>> dest, DataTemplate<TSource, TDest> dataTemplate) : UpdateCollectionModelLinker<TSource, DataTemplateGeneratedValue<TSource, TDest>>(source, dest)
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
class RefRemover<TSrc, TOut>(IReadOnlyUpdateCollection<DataTemplateGeneratedValue<TSrc, TOut>> source, IList<TOut> dest) : UpdateCollectionModelLinker<DataTemplateGeneratedValue<TSrc, TOut>, TOut>(source, dest)
{
    protected override TOut CreateFrom(DataTemplateGeneratedValue<TSrc, TOut> source)
        => source.GeneratedValue;
}
readonly struct Disposable(Action OnDispose) : IDisposable
{
    public void Dispose() => OnDispose();
}