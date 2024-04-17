namespace Get.Data.Bindings;
public interface IBinding<T> : IReadOnlyBinding<T>, IDataBinding<T>, INotifyBinding<T> { }