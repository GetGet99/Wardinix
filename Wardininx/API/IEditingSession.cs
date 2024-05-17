namespace Wardininx.API;

public interface IEditingSession<T>
{
	T Core { get; }
}