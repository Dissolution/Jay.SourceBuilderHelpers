using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // private static unsafe void CopyBlock(char* source, ref char dest, int charCount)
    // {
    //     Emit.Ldarg(nameof(dest));
    //     Emit.Ldarg(nameof(source));
    //     Emit.Ldarg(nameof(charCount));
    //     Emit.Sizeof<char>();
    //     Emit.Mul();
    //     //Emit.Conv_U4();
    //     Emit.Cpblk();
    // }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CopyTo(ReadOnlySpan<char> source, Span<char> dest)
    {
        CopyBlock(in source.GetPinnableReference(),
            ref dest.GetPinnableReference(),
            source.Length);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // internal static unsafe void CopyTo(string source, Span<char> dest)
    // {
    //     fixed (char* sourcePtr = source)
    //     {
    //         CopyBlock(sourcePtr,
    //             ref dest.GetPinnableReference(),
    //             source.Length);
    //     }
    // }

    private static readonly string[] _newLineSeparator = new string[1] { Environment.NewLine };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] Split(string? text, string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        if (text is null) return Array.Empty<string>();
        return text.Split(new string[1] { separator }, options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] SplitLines(string? text, StringSplitOptions options = StringSplitOptions.None)
    {
        if (text is null) return Array.Empty<string>();
        return text.Split(_newLineSeparator, options);
    }

    public delegate void WithLine(ReadOnlySpan<char> line);

    public static void PerLine(this ReadOnlySpan<char> text, WithLine perLine)
    {
        ReadOnlySpan<char> sep = Environment.NewLine.AsSpan();
        int start = 0;
        int index = 0;
        int textLen = text.Length;
        int len;
        while (index < textLen)
        {
            if (text.StartsWith(sep))
            {
                len = index - start;
                if (len > 0)
                {
                    perLine(text.Slice(start, len));
                }

                start = index + sep.Length;
                index = start;
            }
            else
            {
                index++;
            }
        }

        len = index - start;
        if (len > 0)
        {
            perLine(text.Slice(start, len));
        }
    }

    public static List<Range> SplitLines(this ReadOnlySpan<char> text)
    {
        var ranges = new List<Range>();
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
                    ranges.Add(new Range(start: start, end: end));
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
            ranges.Add(new Range(start: start, end: index));
        }

        return ranges;
    }

    public static int FirstIndexOf(this ReadOnlySpan<char> text, char ch, int start = 0)
    {
        int len = text.Length;
        for (var i = start; i < len; i++)
        {
            if (text[i] == ch)
                return i;
        }

        return -1;
    }

    public static string Combine(string first, ReadOnlySpan<char> second)
    {
        int fLen = first.Length;
        Span<char> buffer = stackalloc char[fLen + second.Length];
        first.AsSpan().CopyTo(buffer);
        second.CopyTo(buffer[fLen..]);
        return buffer.ToString();
    }
}