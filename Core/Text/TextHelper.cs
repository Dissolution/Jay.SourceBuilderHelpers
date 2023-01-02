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
    private static unsafe void CopyBlock(char* source, ref char dest, int charCount)
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
    internal static unsafe void CopyTo(string source, Span<char> dest)
    {
        fixed (char* sourcePtr = source)
        {
            CopyBlock(sourcePtr,
                ref dest.GetPinnableReference(),
                source.Length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] Split(string? text, string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        if (text is null) return Array.Empty<string>();
        return text.Split(new string[1] { separator }, options);
    }

    public static List<(int start, int length)> SplitLines(this ReadOnlySpan<char> text)
    {
        var ranges = new List<(int, int)>();
        ReadOnlySpan<char> sep = Environment.NewLine.AsSpan();
        int start = 0;
        int index = 0;
        int len = text.Length;
        while (index < len)
        {
            if (text.StartsWith(sep))
            {
                int end = index;
                if (end - start > 0)
                {
                    ranges.Add((start, end - start));
                }

                start = index + sep.Length;
                index = start;
            }
            else
            {
                index++;
            }
        }

        if (index - start > 0)
        {
            ranges.Add((start, index - start));
        }

        return ranges;
    }
}