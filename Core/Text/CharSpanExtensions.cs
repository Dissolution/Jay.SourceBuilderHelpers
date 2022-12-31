using System;
using System.Collections.Generic;
using System.Text;

namespace Jay.SourceGen.Text;

internal static class CharSpanExtensions
{
    public static ReadOnlySpan<T> SafeSlice<T>(this ReadOnlySpan<T> span, int start, int length)
    {
        if (length == 0) return default;

        int s, l;
        if (length > 0)
        {
            s = start;
            l = length;
        }
        else //if (length < 0)
        {
            l = -length;
            s = start - l;
        }
        // If our start is negative
        if (s < 0)
        {
            // Adjust length back by that amount
            l += s; // s is negative
            // And floor start
            s = 0;
        }
        // If our start + length runs over
        int e = s + l;
        if (e > span.Length)
        {
            // Adjust length back by that amount
            l -= (e - span.Length);
        }
        // We have a safe slice
        return span.Slice(s, l);
    }
}