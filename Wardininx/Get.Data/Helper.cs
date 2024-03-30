namespace Get.Data.Helpers;

public static class Extension
{
    public static T WithCustomCode<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
    public static T WithCustomCode<T, TOut>(this T value, Func<T, TOut> func, out TOut outVar)
    {
        outVar = func(value);
        return value;
    }
    public static T AssignTo<T>(this T value, out T variable)
    {
        variable = value;
        return value;
    }
}
