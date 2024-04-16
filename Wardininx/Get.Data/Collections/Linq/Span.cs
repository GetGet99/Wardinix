using Get.Data.Collections;

namespace Get.Data.Collections.Linq;

public static partial class Extension {
    internal static IGDReadOnlyCollection<T> Span<T>(IGDReadOnlyCollection<T> c, Range range)
        => new SpanHandler<T>(c, range);
}

readonly struct SpanHandler<T>(IGDReadOnlyCollection<T> c, Range range) : IGDReadOnlyCollection<T>
{
    // probably should do a bit of caching
    public T this[int index] => c[range.GetOffsetAndLength(Count).Offset + index];
    public int Count => range.GetOffsetAndLength(Count).Length;
}