using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Wardininx;

public static class ToDo
{
    [DoesNotReturn]
    public static void NotImplemented()
    {
        throw new NotImplementedException();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Note()
    {
        
    }
    [DoesNotReturn]
    public static T NotImplemented<T>()
    {
        throw new NotImplementedException();
    }
}