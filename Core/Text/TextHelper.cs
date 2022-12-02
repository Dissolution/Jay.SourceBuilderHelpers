using System.Runtime.CompilerServices;
using static InlineIL.IL;

namespace Jay.SourceGen.Text;

public static class TextHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CopyBlock(in char source, ref char dest, int charCount)
    {
        Emit.Ldarg(nameof(dest));
        Emit.Ldarg(nameof(source));
        Emit.Ldarg(nameof(charCount));
        Emit.Sizeof<char>();
        Emit.Mul();
        //Emit.Conv_U4();
        Emit.Cpblk();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CopyTo(ReadOnlySpan<char> source, Span<char> dest)
    {
        CopyBlock(in source.GetPinnableReference(),
            ref dest.GetPinnableReference(),
            source.Length);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] Split(string? text, string separator, StringSplitOptions options = default)
    {
        if (text is null) return Array.Empty<string>();
#if NETSTANDARD2_0
        return text.Split(new string[1] { separator }, options);
#elif NETSTANDARD2_1
        return text.Split(separator, options);
#endif
    }
}