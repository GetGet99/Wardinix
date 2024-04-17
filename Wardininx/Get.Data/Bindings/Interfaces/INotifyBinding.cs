using Get.Data.Properties;
namespace Get.Data.Bindings;
public interface INotifyBinding<T>
{
    event Action RootChanged;
    event ValueChangingHandler<T>? ValueChanging;
    event ValueChangedHandler<T>? ValueChanged;
}