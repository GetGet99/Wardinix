namespace Get.Data.Bindings.Linq;
partial class Extension
{
    public static IBinding<T> AllowWritebackWhen<T>(this IBinding<T> src, Func<bool> func)
        => src.Select(x => x, (T output, T oldInput) => func() ? output : oldInput);
    public static IBinding<T> AllowWritebackWhen<T>(this IBinding<T> src, Func<T, bool> func)
        => src.Select(x => x, (T output, T oldInput) => func(output) ? output : oldInput);
}