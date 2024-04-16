using Get.Data.Collections.Conversion;
using Get.Data.Collections;

public static class Collection
{
    struct SingleCollection<T>(T item) : IGDFixedSizeCollection<T>
    {
        public T this[int index] { readonly get => item; set => item = value; }
        public readonly int Count => 1;
    }
    public static IGDFixedSizeCollection<T> Single<T>(T item)
        => new SingleCollection<T>(item);
    public static IGDFixedSizeCollection<T> Array<T>(params T[] items)
        => items.AsGDFixedSizeCollection();
}