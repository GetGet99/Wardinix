namespace Get.Data.Bindings;
public interface IDataBinding<T> : IReadOnlyDataBinding<T>
{
    new T CurrentValue { get; set; }
}