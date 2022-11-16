using System.Runtime.CompilerServices;

using static InlineIL.IL;

namespace Benchmarks;

public static class TextHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyTo(in char source, ref char destination, int charCount)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(charCount));
        Emit.Sizeof<char>();
        Emit.Mul();
        Emit.Cpblk();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void CopyTo(char* source, char* destination, int charCount)
    {
        Emit.Ldarg(nameof(destination));
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(charCount));
        Emit.Sizeof<char>();
        Emit.Mul();
        Emit.Cpblk();
    }
}