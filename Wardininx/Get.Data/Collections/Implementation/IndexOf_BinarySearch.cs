#nullable enable
namespace Get.Data.Collections.Implementation;
partial class Implementations
{
    public static int IndexOf_BinarySearch<T>(IGDReadOnlyCollection<T> collection, T item, IComparer<T>? comparer = null)
        => BinarySearch(collection, item, out var idx, comparer: comparer) ? -1 : idx;
    public enum BinarySearchMode
    {
        /// <summary>
        /// If there are no matches, set bool to false and set index to -1
        /// </summary>
        Exact,
        /// <summary>
        /// If there are no matches, set bool to false and set index to the greatest number that is less than item.
        /// If there is still no such item (such as empty collection), set index to -1.
        /// </summary>
        Lower,
        /// <summary>
        /// If there are no matches, set bool to false and set index to the lowest number that is more than item.
        /// If there is still no such item (such as empty collection), set index to -1.
        /// </summary>
        Higher
    }
    public static bool BinarySearch<T>(IGDReadOnlyCollection<T> collection, T item, out int index, BinarySearchMode mode = BinarySearchMode.Exact, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;

        int low = 0;
        int high = collection.Count - 1;
        index = -1;
        bool found = false;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            int comparison = comparer.Compare(collection[mid], item);

            if (comparison == 0)
            {
                found = true;
                index = mid;
                break;
            }
            else if (comparison < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        if (!found)
        {
            if (mode == BinarySearchMode.Lower)
            {
                index = high;
            }
            else if (mode == BinarySearchMode.Higher)
            {
                index = low;
            }
        }

        return found;
    }
}