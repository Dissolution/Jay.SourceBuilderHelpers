using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static  InlineIL.IL;

namespace Jay.SourceBuilderHelpers.Text;

public static class TextHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void CopyBlock(char* sourcePtr, char* destPtr, int charCount)
    {
        Emit.Ldarg(nameof(destPtr));
        Emit.Ldarg(nameof(sourcePtr));
        Emit.Ldarg(nameof(charCount));
        Emit.Sizeof<char>();
        Emit.Mul();
        //Emit.Conv_U4();
        Emit.Cpblk();
    }


    /// <summary>
    /// WARNING: No bounds checks of any kind happen here
    /// </summary>
    internal static void CopyTo(string text, char[] dest, int destOffset, int charCount)
    {
        unsafe
        {
            fixed (char* sourcePtr = text)
            fixed (char* destPtr = &dest[destOffset])
            {
                CopyBlock(sourcePtr, destPtr, charCount);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(string? x, string? y)
    {
        return x == y;
    }

    public static bool Equals(string str, char[] chars, int charsOffset)
    {
        if ((uint)charsOffset >= chars.Length)
            throw new ArgumentOutOfRangeException(nameof(charsOffset));
        if (str.Length + charsOffset > chars.Length) return false;
        for (var s = 0; s < str.Length; s++, charsOffset++)
        {
            if (chars[charsOffset] != str[s]) return false;
        }
        return true;
    }


    public static bool Equals(string? x, char[]? y)
    {
        if (x is null) return y is null;
        if (y is null) return false;
        int len = x.Length;
        if (y.Length != len) return false;
        for (var i = 0; i < len; i++)
        {
            if (x[i] != y[i]) return false;
        }
        return true;
    }

    public static bool Equals(char[]? x, string? y)
    {
        if (x is null) return y is null;
        if (y is null) return false;
        int len = x.Length;
        if (y.Length != len) return false;
        for (var i = 0; i < len; i++)
        {
            if (x[i] != y[i]) return false;
        }
        return true;
    }

    public static bool Equals(char[]? x, char[]? y)
    {
        if (x is null) return y is null;
        if (y is null) return false;
        int len = x.Length;
        if (y.Length != len) return false;
        for (var i = 0; i < len; i++)
        {
            if (x[i] != y[i]) return false;
        }
        return true;
    }

    public static bool TryCopyTo(string? source, char[] destination)
    {
        if (source is null) return true;
        int len = source.Length;
        if (len > destination.Length) return false;
        unsafe
        {
            fixed (char* sourcePtr = source)
            fixed (char* destPtr = destination)
            {
                CopyBlock(sourcePtr, destPtr, len);
            }
        }
        return true;
    }

    public static bool TryCopyTo(string? source, char[] destination, int destOffset)
    {
        if (source is null) return true;
        if ((uint)destOffset >= destination.Length) return false;
        int len = source.Length;
        if (len + destOffset > destination.Length) return false;
        unsafe
        {
            fixed (char* sourcePtr = source)
            fixed (char* destPtr = &destination[destOffset])
            {
                CopyBlock(sourcePtr, destPtr, len);
            }
        }
        return true;
    }
}