namespace Get.Data.Collections;
public interface IGDCollection<T> : IGDFixedSizeCollection<T>
{
    void Insert(int index, T item);
    void RemoveAt(int index);
}