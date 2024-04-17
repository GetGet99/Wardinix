namespace Get.Data.Properties;
public interface IPropertyDefinition<TOwnerType, TPropertyType>
{
    PropertyBase<TPropertyType> GetProperty(TOwnerType owner);
}
public abstract class PropertyDefinitionBase<TOwnerType, TPropertyType> : IPropertyDefinition<TOwnerType, TPropertyType>
{
    public abstract PropertyBase<TPropertyType> GetProperty(TOwnerType owner);

    public PropertyDefinition<TNewOwnerType, TPropertyType> As<TNewOwnerType>() where TNewOwnerType : TOwnerType
        => new(x => GetProperty(x));

    public PropertyDefinition<TNewOwnerType, TPropertyType> As<TNewOwnerType>(Func<TNewOwnerType, TOwnerType> caster)
        => new(x => GetProperty(caster(x)));

    public PropertyDefinition<TOwnerType, TNewPropertyType> WithConverter<TNewPropertyType>(Func<TPropertyType, TNewPropertyType> forwardConvert)
        => new(
            x => new PropertyWithConverter<TPropertyType, TNewPropertyType>(GetProperty(x), forwardConvert)
        );
    public PropertyDefinition<TOwnerType, TNewPropertyType> WithConverter<TNewPropertyType>(Func<TPropertyType, TNewPropertyType> forwardConvert, Func<TNewPropertyType, TPropertyType> backwardConvert)
        => new(
            x => new PropertyWithConverter<TPropertyType, TNewPropertyType>(GetProperty(x), forwardConvert, backwardConvert)
        );
}

public sealed class PropertyDefinition<TOwnerType, TPropertyType>(Func<TOwnerType, PropertyBase<TPropertyType>> getProperty) : PropertyDefinitionBase<TOwnerType, TPropertyType>
{
    public override PropertyBase<TPropertyType> GetProperty(TOwnerType owner)
        => getProperty(owner);
}
static class PropertyDefinition
{
    public static PropertyDefinition<TOwnerType, TPropertyType> Create<TOwnerType, TPropertyType>(Func<TOwnerType, PropertyBase<TPropertyType>> getProperty)
        => new(getProperty);
    public static PropertyDefinition<TOwnerType, TPropertyType> CreateExpr<TOwnerType, TPropertyType>(Func<TOwnerType, TPropertyType> getter, Action<TOwnerType, TPropertyType> setter, bool automaticNotifyOnSetValue = true)
        => Create<TOwnerType, TPropertyType>(owner => new ExprProperty<TPropertyType>(() => getter(owner), (x => setter(owner, x)), automaticNotifyOnSetValue));
    public static PropertyDefinition<TOwnerType, TPropertyType> CreateExpr<TOwnerType, TPropertyType>(Func<TOwnerType, TPropertyType> getter, Action<TOwnerType, TPropertyType> setter, Func<bool> writebackCondition, bool automaticNotifyOnSetValue = true)
        => CreateExpr(getter, (x, y) =>
        {
            if (writebackCondition()) setter(x, y);
        }, automaticNotifyOnSetValue: automaticNotifyOnSetValue);
}