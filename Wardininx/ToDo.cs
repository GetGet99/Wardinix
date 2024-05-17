using System.Diagnostics.CodeAnalysis;

namespace Wardininx;

public static class ToDo
{
    [DoesNotReturn]
    public static void NotImplemented()
    {
        throw new NotImplementedException();
    }
    [DoesNotReturn]
    public static T NotImplemented<T>()
    {
        throw new NotImplementedException();
    }
}