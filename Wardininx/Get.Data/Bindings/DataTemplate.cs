using Get.Data.Collections;
using System.Collections.Generic;

namespace Get.Data.Bindings;

public delegate TOut DataTemplateDefinition<TSrc, TOut>(RootBinding<TSrc> dataRoot);
public class DataTemplateWithIndex<TSrc, TOut>(DataTemplateDefinition<IndexItem<TSrc>, TOut> TemplateDefinition) :
    DataTemplate<IndexItem<TSrc>, TOut>(TemplateDefinition)
{

}
public class DataTemplate<TSrc, TOut>(DataTemplateDefinition<TSrc, TOut> TemplateDefinition)
{
    readonly internal DataTemplateDefinition<TSrc, TOut> TemplateDefinition = TemplateDefinition;
    readonly Queue<DataTemplateGeneratedValue<TSrc, TOut>> recycledQueue = new();
    public DataTemplateGeneratedValue<TSrc, TOut> Generate(IReadOnlyBinding<TSrc> source)
        => new(this, source);
    public DataTemplateGeneratedValue<TSrc, TOut> Generate(TSrc source)
        => new(this, new ValueBinding<TSrc>(source));
    internal void NotifyRecycle(DataTemplateGeneratedValue<TSrc, TOut> recylcedItem)
    {
        recycledQueue.Enqueue(new(recylcedItem.Template, recylcedItem.DataRoot, recylcedItem.GeneratedValue));
    }
}
public class DataTemplateGeneratedValue<TSrc, TOut>
{
    internal DataTemplateGeneratedValue(DataTemplate<TSrc, TOut> Template, IReadOnlyBinding<TSrc> binding)
    {
        this.Template = Template;
        DataRoot = new(binding);
        GeneratedValue = Template.TemplateDefinition(DataRoot);
    }
    internal DataTemplateGeneratedValue(DataTemplate<TSrc, TOut> Template, RootBinding<TSrc> dataRoot, TOut value)
    {
        this.Template = Template;
        DataRoot = dataRoot;
        GeneratedValue = value;
    }
    public IReadOnlyBinding<TSrc> Binding { get => DataRoot.ParentBinding; set => DataRoot.ParentBinding = value; }
    public DataTemplate<TSrc, TOut> Template { get; }
    public TOut GeneratedValue { get; private set; }
    internal readonly RootBinding<TSrc> DataRoot;
    public void Recycle()
    {
        Template.NotifyRecycle(this);
        //Value = default!;
    }
}
