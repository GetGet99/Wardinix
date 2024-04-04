using Get.Data.Bindings;

namespace Get.Data.Collections;

static class Extension
{
    public static TwoWayConverterCollection<TDest, TSrc> WithConverter<TSrc, TDest>(
        this IUpdateCollection<TSrc> src,
        ForwardConverter<TSrc, TDest> forwardConverter,
        BackwardConverter<TSrc, TDest> backwardConverter
    ) => new(src, forwardConverter, backwardConverter);
    public static OneWayConverterCollection<TDest, TSrc> WithForwardConverter<TSrc, TDest>(
        this IReadOnlyUpdateCollection<TSrc> src,
        ForwardConverter<TSrc, TDest> forwardConverter
    ) => new(src, forwardConverter);
    public static OneWayConverterCollection<TDest, TSrc> WithForwardConverter<TSrc, TDest>(
        this IUpdateCollection<TSrc> src,
        ForwardConverter<TSrc, TDest> forwardConverter
    ) => src.AsReadOnly().WithForwardConverter(forwardConverter);
    public static ReadOnlyUpdateCollection<T> AsReadOnly<T>(
        this IUpdateCollection<T> src
    ) => new(src);
    public static IndexReadOnlyUpdateCollection<T> WithIndex<T>(
        this IReadOnlyUpdateCollection<T> src
    ) => new(src);
    public static IndexReadOnlyUpdateCollection<T> WithIndex<T>(
        this IUpdateCollection<T> src
    ) => src.AsReadOnly().WithIndex();

}