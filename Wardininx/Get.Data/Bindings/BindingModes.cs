namespace Get.Data.Bindings;
public enum ReadOnlyBindingModes
{
    /// <summary>
    /// Only change when data root is changed
    /// </summary>
    OneTime,
    OneWay,
    /// <summary>
    /// Same as <see cref="OneWay"/>
    /// </summary>
    OneWayToTarget = OneWay
}
public enum BindingModes
{
    OneWayToSource,
    /// <summary>
    /// Two way binding, update the target value immedietely as the binding is set
    /// </summary>
    TwoWay,
    /// <summary>
    /// Same as <see cref="TwoWay"/>
    /// </summary>
    TwoWayUpdateTargetImmediete = TwoWay,
    /// <summary>
    /// Two way binding, update the source value immedietely as the binding is set
    /// </summary>
    TwoWayUpdateSourceImmediete
}