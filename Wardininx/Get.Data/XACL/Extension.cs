using Get.Data.Bindings;
using Get.Data.Collections;
using Get.Data.Collections.Conversion;
using Get.Data.Collections.Linq;
using Get.Data.Collections;
using Get.Data.Collections.Update;
using Get.Data.Properties;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Get.Data.XACL;
interface ICollectionItemsBinding<TTarget> { }
public record class CollectionItemsBinding<TSrc, TTarget>(IUpdateReadOnlyCollection<TSrc> Source, DataTemplate<TSrc, TTarget> DataTemplate) : ICollectionItemsBinding<TTarget>
{

}
public record class CollectionItemsBinding<T>(IUpdateReadOnlyCollection<T> Source)
{
    
}
public static class CollectionItemsBinding
{
    public static CollectionItemsBinding<T> Create<T>(IUpdateReadOnlyCollection<T> Source) => new(Source);
    public static CollectionItemsBinding<TSrc, TTarget> Create<TSrc, TTarget>(IUpdateReadOnlyCollection<TSrc> Source, DataTemplate<TSrc, TTarget> DataTemplate) => new(Source, DataTemplate);
}
public static class XACLExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<TSrc, T>(this IList<T> collection, CollectionItemsBinding<TSrc, T> toBind)
    {
        toBind.Source.Bind(collection.AsGDCollection(), toBind.DataTemplate);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T, TTarget>(this IList<TTarget> collection, CollectionItemsBinding<T> toBind) where T : TTarget
    {
        toBind.Source.Select(x => (TTarget)x).Bind(collection.AsGDCollection());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<TSrc, TTarget>(this IList<TTarget> collection, IUpdateReadOnlyCollection<TSrc> source, DataTemplate<TSrc, TTarget> dataTemplate)
    {
        source.Bind(collection.AsGDCollection(), dataTemplate);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T>(this IList<T> collection, IUpdateReadOnlyCollection<T> source)
    {
        source.Bind(collection.AsGDCollection());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<TSrc, T>(this IGDCollection<T> collection, CollectionItemsBinding<TSrc, T> toBind)
    {
        toBind.Source.Bind(collection, toBind.DataTemplate);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T, TTarget>(this IGDCollection<TTarget> collection, CollectionItemsBinding<T> toBind) where T : TTarget
    {
        toBind.Source.Select(x => (TTarget)x).Bind(collection);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<TSrc, TTarget>(this IGDCollection<TTarget> collection, IUpdateReadOnlyCollection<TSrc> source, DataTemplate<TSrc, TTarget> dataTemplate)
    {
        source.Bind(collection, dataTemplate);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T>(this IGDCollection<T> collection, IUpdateReadOnlyCollection<T> source)
    {
        source.Bind(collection);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithBinding<TSrc>(this TSrc src, XACLBindings<TSrc> Binding)
    {
        Binding.ApplyTo(src);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithOneWayBinding<TSrc>(this TSrc src, GenericReadOnlyBindingKinds<TSrc> Binding)
    {
        Binding.ApplyTo(src, ReadOnlyBindingModes.OneWay);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithOneTimeBinding<TSrc>(this TSrc src, GenericReadOnlyBindingKinds<TSrc> Binding)
    {
        Binding.ApplyTo(src, ReadOnlyBindingModes.OneTime);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithTwoWayBinding<TSrc>(this TSrc src, GenericBindingKinds<TSrc> Binding)
    {
        Binding.ApplyTo(src, BindingModes.TwoWay);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithOneWayToSourceBinding<TSrc>(this TSrc src, GenericBindingKinds<TSrc> Binding)
    {
        Binding.ApplyTo(src, BindingModes.OneWayToSource);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithOneWayToTargetBinding<TSrc>(this TSrc src, GenericReadOnlyBindingKinds<TSrc> Binding)
    {
        Binding.ApplyTo(src, ReadOnlyBindingModes.OneWayToTarget);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithTwoWayUpdateTargetImmedieteBinding<TSrc>(this TSrc src, GenericBindingKinds<TSrc> Binding)
    {
        Binding.ApplyTo(src, BindingModes.TwoWayUpdateTargetImmediete);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithTwoWayUpdateSourceImmedieteBinding<TSrc>(this TSrc src, GenericBindingKinds<TSrc> Binding)
    {
        Binding.ApplyTo(src, BindingModes.TwoWayUpdateSourceImmediete);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithOneWayBinding<TSrc, TProp>(this TSrc src, PropertyDefinitionBase<TSrc, TProp> srcPropDef, IReadOnlyBinding<TProp> binding)
    {
        srcPropDef.GetProperty(src).Bind(binding, ReadOnlyBindingModes.OneWay);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithOneTimeBinding<TSrc, TProp>(this TSrc src, PropertyDefinitionBase<TSrc, TProp> srcPropDef, IReadOnlyBinding<TProp> binding)
    {
        srcPropDef.GetProperty(src).Bind(binding, ReadOnlyBindingModes.OneTime);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithTwoWayBinding<TSrc, TProp>(this TSrc src, PropertyDefinitionBase<TSrc, TProp> srcPropDef, IBinding<TProp> binding)
    {
        srcPropDef.GetProperty(src).Bind(binding, BindingModes.TwoWay);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithOneWayToSourceBinding<TSrc, TProp>(this TSrc src, PropertyDefinitionBase<TSrc, TProp> srcPropDef, IBinding<TProp> binding)
    {
        srcPropDef.GetProperty(src).Bind(binding, BindingModes.OneWayToSource);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithOneWayToTargetBinding<TSrc, TProp>(this TSrc src, PropertyDefinitionBase<TSrc, TProp> srcPropDef, IReadOnlyBinding<TProp> binding)
    {
        srcPropDef.GetProperty(src).Bind(binding, ReadOnlyBindingModes.OneWayToTarget);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithTwoWayUpdateTargetImmedieteBinding<TSrc, TProp>(this TSrc src, PropertyDefinitionBase<TSrc, TProp> srcPropDef, IBinding<TProp> binding)
    {
        srcPropDef.GetProperty(src).Bind(binding, BindingModes.TwoWayUpdateTargetImmediete);
        return src;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSrc WithTwoWayUpdateSourceImmedieteBinding<TSrc, TProp>(this TSrc src, PropertyDefinitionBase<TSrc, TProp> srcPropDef, IBinding<TProp> binding)
    {
        srcPropDef.GetProperty(src).Bind(binding, BindingModes.TwoWayUpdateSourceImmediete);
        return src;
    }
    public static XACLSyntaxHelper<TSrc> XACL<TSrc>(this TSrc source)
    {
        return new XACLSyntaxHelper<TSrc>(source);
    }
}
public class XACLSyntaxHelper<TSrc>(TSrc src)
{
    public TSrc Source { get; } = src;
    XACLBindings<TSrc>? bindings;
    public XACLBindings<TSrc> Bindings => bindings ??= new();

    public static implicit operator TSrc(XACLSyntaxHelper<TSrc> xaclsrc) => xaclsrc.Source;
}
public class XACLBindings<TOwner>
{
    ReadOnlyBindingKinds<TOwner>? oneTime, oneWay;
    BindingKinds<TOwner>? oneWayToSource, twoWay, twoWayUpdateSourceImmediete;
    public ReadOnlyBindingKinds<TOwner> OneTime => oneTime ??= new(ReadOnlyBindingModes.OneTime);
    public ReadOnlyBindingKinds<TOwner> OneWay => oneWay ??= new(ReadOnlyBindingModes.OneWay);
    public ReadOnlyBindingKinds<TOwner> OneWayToTarget => OneWay;
    public BindingKinds<TOwner> OneWayToSource => oneWayToSource ??= new(BindingModes.OneWayToSource);
    public BindingKinds<TOwner> TwoWay => twoWay ??= new(BindingModes.TwoWay);
    public BindingKinds<TOwner> TwoWayUpdateTargetImmediete => TwoWay;
    public BindingKinds<TOwner> TwoWayUpdateSourceImmediete => twoWayUpdateSourceImmediete ??= new(BindingModes.TwoWayUpdateSourceImmediete);
    public void ApplyTo(TOwner owner)
    {
        oneTime?.ApplyTo(owner);
        oneWay?.ApplyTo(owner);
        oneWayToSource?.ApplyTo(owner);
        twoWay?.ApplyTo(owner);
        twoWayUpdateSourceImmediete?.ApplyTo(owner);
    }
}
public class BindingKinds<TOwner>(BindingModes bindingModes) : IEnumerable
{
    Action<TOwner>? setBinding;
    public void Add<TProp>(PropertyDefinitionBase<TOwner, TProp> propertyDefinition, IBinding<TProp> binding)
    {
        setBinding += x => propertyDefinition.GetProperty(x).Bind(binding, bindingModes);
    }
    public void ApplyTo(TOwner owner) => setBinding?.Invoke(owner);

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new InvalidOperationException($"{nameof(IEnumerable.GetEnumerator)} is not implemented because BindingKinds is not supposed to be used as collection. We just implement so C# syntax sugar works");
    }
}
public class ReadOnlyBindingKinds<TOwner>(ReadOnlyBindingModes bindingModes) : IEnumerable
{
    Action<TOwner>? setBinding;
    public void Add<TProp>(PropertyDefinitionBase<TOwner, TProp> propertyDefinition, IReadOnlyBinding<TProp> binding)
    {
        setBinding += x => propertyDefinition.GetProperty(x).Bind(binding, bindingModes);
    }
    public void ApplyTo(TOwner owner) => setBinding?.Invoke(owner);

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new InvalidOperationException($"{nameof(IEnumerable.GetEnumerator)} is not implemented because BindingKinds is not supposed to be used as collection. We just implement so C# syntax sugar works");
    }
}
public class GenericBindingKinds<TOwner> : IEnumerable
{
    Action<TOwner, BindingModes>? setBinding;

    public void Add<TProp>(PropertyDefinitionBase<TOwner, TProp> propertyDefinition, IBinding<TProp> binding)
    {
        setBinding += (x, bm) =>
        {
            propertyDefinition.GetProperty(x).Bind(binding, bm);
        };
    }
    public void ApplyTo(TOwner owner, BindingModes bindingModes) => setBinding?.Invoke(owner, bindingModes);

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new InvalidOperationException($"{nameof(IEnumerable.GetEnumerator)} is not implemented because BindingKinds is not supposed to be used as collection. We just implement so C# syntax sugar works");
    }
}
public class GenericReadOnlyBindingKinds<TOwner> : IEnumerable
{
    Action<TOwner, ReadOnlyBindingModes>? setBinding;

    public void Add<TProp>(PropertyDefinitionBase<TOwner, TProp> propertyDefinition, IReadOnlyBinding<TProp> binding)
    {
        setBinding += (x, bm) =>
        {
            propertyDefinition.GetProperty(x).Bind(binding, bm);
        };
    }
    public void ApplyTo(TOwner owner, ReadOnlyBindingModes bindingModes) => setBinding?.Invoke(owner, bindingModes);

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new InvalidOperationException($"{nameof(IEnumerable.GetEnumerator)} is not implemented because BindingKinds is not supposed to be used as collection. We just implement so C# syntax sugar works");
    }
}