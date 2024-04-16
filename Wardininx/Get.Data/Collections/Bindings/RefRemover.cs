using Get.Data.Bindings;
using Get.Data.Collections.Update;
using Get.Data.ModelLinker;
namespace Get.Data.Collections;
class RefRemover<TSrc, TOut>(IUpdateReadOnlyCollection<DataTemplateGeneratedValue<TSrc, TOut>> source, IGDCollection<TOut> dest) : UpdateCollectionModelLinker<DataTemplateGeneratedValue<TSrc, TOut>, TOut>(source, dest)
{
    protected override TOut CreateFrom(DataTemplateGeneratedValue<TSrc, TOut> source)
        => source.GeneratedValue;
}