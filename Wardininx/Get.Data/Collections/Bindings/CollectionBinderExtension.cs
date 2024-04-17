using Get.Data.Bindings;
using Get.Data.Collections.Update;
using Get.Data.ModelLinker;
namespace Get.Data.Collections;
public static class CollectionBinderExtension
{
    public static IDisposable Bind<TSrc, TOut>(this IUpdateReadOnlyCollection<TSrc> collection, IGDCollection<TOut> @out, DataTemplate<TSrc, TOut> dataTemplate)
    {
        UpdateCollection<DataTemplateGeneratedValue<TSrc, TOut>> middleCollection = new();
        var a = new TemplateLinker<TSrc, TOut>(collection, middleCollection, dataTemplate);
        var b = new RefRemover<TSrc, TOut>(middleCollection, @out);
        a.ResetAndReadd();
        //b.ResetAndReadd();
        return new Disposable(() =>
        {
            a.Dispose();
            b.Dispose();
            @out.Clear();
        });
    }
    public static IDisposable Bind<T>(this IUpdateCollection<T> collection, IGDCollection<T> @out)
        => collection.Bind(@out);
    public static IDisposable Bind<T>(this IUpdateReadOnlyCollection<T> collection, IGDCollection<T> @out)
    {
        var linker = new UpdateCollectionModelLinker<T>(collection, @out);
        linker.ResetAndReadd();
        return new Disposable(() =>
        {
            linker.Dispose();
            @out.Clear();
        });
    }
}