using Get.Data.Bindings;
using Get.Data.Collections.Update;
using Get.Data.ModelLinker;
namespace Get.Data.Collections;

class TemplateLinker<TSource, TDest>(IUpdateReadOnlyCollection<TSource> source, UpdateCollection<DataTemplateGeneratedValue<TSource, TDest>> dest, DataTemplate<TSource, TDest> dataTemplate) : UpdateCollectionModelLinker<TSource, DataTemplateGeneratedValue<TSource, TDest>>(source, dest)
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