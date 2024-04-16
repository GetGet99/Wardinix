namespace Get.Data.Collections;
public interface IGDFixedSizeCollection<T> : IGDReadOnlyCollection<T>
{
    new T this[int index] { get; set; }
}