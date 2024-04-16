using Get.Data.Collections;

namespace Get.Data.Collections.Conversion;
readonly struct ArrayImpl<T>(T[] values) : IGDFixedSizeCollection<T>
{
    public T this[int index]
    {
        get => values[index];
        set => values[index] = value;
    }
    public int Count => values.Length;
}
partial class Extension
{
    public static IGDFixedSizeCollection<T> AsGDFixedSizeCollection<T>(this T[] readOnlyList)
        => new ArrayImpl<T>(readOnlyList);
}