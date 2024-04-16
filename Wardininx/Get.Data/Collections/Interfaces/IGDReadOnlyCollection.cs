namespace Get.Data.Collections;
public interface IGDReadOnlyCollection<T>
{
    T this[int index] { get; }
    int Count { get; }
}