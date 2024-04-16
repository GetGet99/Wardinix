#nullable enable
using Get.Data.Collections.Implementation;

namespace Get.Data.Collections.Implementation
{
    interface IContainsImplGDCollection<T> : IGDCollection<T>
    {
        bool Contains(T item);
    }
}
namespace Get.Data.Collections
{
    partial class DefaultImplementations
    {
        public static bool Contains<T>(this IGDCollection<T> collection, T item)
            => collection is IContainsImplGDCollection<T> impl
            ? impl.Contains(item) : Implementations.Contains_IndexOf(collection, item);
    }
}
