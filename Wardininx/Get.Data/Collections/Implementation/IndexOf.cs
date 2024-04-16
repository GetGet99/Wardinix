#nullable enable
using Get.Data.Collections.Implementation;

namespace Get.Data.Collections.Implementation
{
    interface IIndexOfImplGDCollection<T> : IGDReadOnlyCollection<T>
    {
        /// <summary>
        /// Find the index of the item in the list
        /// </summary>
        /// <param name="item">The item to find the index</param>
        /// <returns>The index of the item. If not found, a negative number is returned.</returns>
        int IndexOf(T item);
    }
}
namespace Get.Data.Collections
{
    partial class DefaultImplementations
    {
        /// <summary>
        /// Find the index of the item in the list
        /// </summary>
        /// <param name="collection">The source collection</param>
        /// <param name="item">The item to find the index</param>
        /// <returns>The index of the item. If not found, a negative number is returned.</returns>
        public static int IndexOf<T>(this IGDReadOnlyCollection<T> collection, T item)
            => collection is IIndexOfImplGDCollection<T> impl
            ? impl.IndexOf(item) : Implementations.IndexOf_LinearSearch(collection, item);
    }
}