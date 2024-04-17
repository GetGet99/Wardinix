namespace Get.Data.Bindings;
public interface IReadOnlyDataBinding<T>
{
    T CurrentValue { get; }
}