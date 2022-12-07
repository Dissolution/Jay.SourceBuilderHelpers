using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Jay.SourceGen.Text;

/// <summary>
/// A utility to write text-like values to pooled <c>char[]</c> instances
/// </summary>
public sealed class CharArrayWriter : IDisposable
{
    private char[] _charArray;
    private int _length;
    
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _charArray.Length;
    }

    public Span<char> Written
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _charArray.AsSpan(0, _length);
    }

    public Span<char> Available
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _charArray.AsSpan(_length);
    }

    /// <summary>
    /// Gets a <c>ref</c> to the <see cref="char"/> at the given <paramref name="index"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than <see cref="Length"/></exception>
    public ref char this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index < Length)
            {
                return ref _charArray[index];
            }
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and {Length - 1}");
        }
    }

    /// <summary>
    /// Gets the current number of <see cref="char"/>acters written
    /// </summary>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
        set => _length = value.Clamp(0, Capacity);
    }

    /// <summary>
    /// Creates a new instance of a <see cref="CharArrayWriter"/>
    /// </summary>
    public CharArrayWriter()
    {
        _charArray = ArrayPool<char>.Shared.Rent(1024);
        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int additionalChars)
    {
        int newCapacity = ((Capacity + additionalChars) * 2).Clamp(1024, Constants.Pool_MaxCapacity);
        var newArray = ArrayPool<char>.Shared.Rent(newCapacity);
        TextHelper.CopyTo(Written, newArray);
        ArrayPool<char>.Shared.Return(_charArray);
        _charArray = newArray;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowWrite(char ch)
    {
        int i = _length;
        Debug.Assert(i + 1 >= Capacity);
        Grow(1);
        _charArray[i] = ch;
        _length = i + 1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowWrite(string text)
    {
        Debug.Assert(text is not null);
        int i = _length;
        int len = text!.Length;
        Debug.Assert(i + len > Capacity);
        Grow(len);
        TextHelper.CopyTo(text.AsSpan(), Available);
        _length = i + len;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowWrite(ReadOnlySpan<char> text)
    {
        int i = _length;
        int len = text.Length;
        Debug.Assert(i + len > Capacity);
        Grow(len);
        TextHelper.CopyTo(text, Available);
        _length = i + len;
    }

    public void Write(char ch)
    {
        int i = _length;
        var chars = _charArray;
        if (i < chars.Length)
        {
            chars[i] = ch;
            _length = i + 1;
        }
        else
        {
            GrowWrite(ch);
        }
    }

    public void Write(string? text)
    {
        if (text is not null)
        {
            int len = text.Length;
            var available = Available;
            if (len <= available.Length)
            {
                TextHelper.CopyTo(text.AsSpan(), available);
                _length += len;
            }
            else
            {
                GrowWrite(text);
            }
        }
    }

    public void Write(ReadOnlySpan<char> text)
    {
        int len = text.Length;
        var available = Available;
        if (len <= available.Length)
        {
            TextHelper.CopyTo(text, available);
            _length += len;
        }
        else
        {
            GrowWrite(text);
        }
    }

    public void Write<T>(T? value)
    {
        if (value is IFormattable)
        {
            Write(((IFormattable)value).ToString(default, default));
        }
        else
        {
            Write(value?.ToString());
        }
    }

    public void WriteFormat<T>(T? value, string? format, IFormatProvider? provider = null)
    {
        if (value is IFormattable)
        {
            Write(((IFormattable)value).ToString(format, provider));
        }
        else
        {
            Write(value?.ToString());
        }
    }

    public void WriteDelimited<T>(string delimiter, IEnumerable<T> values)
    {
        using var e = values.GetEnumerator();
        if (!e.MoveNext()) return;
        Write<T>(e.Current);
        while (e.MoveNext())
        {
            Write(delimiter);
            Write<T>(e.Current);
        }
    }

    public void Delimited<T>(string delimiter, IEnumerable<T> values, Action<CharArrayWriter, T> writeValue)
    {
        using var e = values.GetEnumerator();
        if (!e.MoveNext()) return;
        writeValue(this, e.Current);
        while (e.MoveNext())
        {
            Write(delimiter);
            writeValue(this, e.Current);
        }
    }

    public bool StartsWith(char ch)
    {
        return _length > 0 && _charArray[0] == ch;
    }

    public bool StartsWith(string text) => StartsWith(text.AsSpan());

    public bool StartsWith(ReadOnlySpan<char> text) => Written.StartsWith(text);

    public bool EndsWith(char ch)
    {
        return _length > 0 && _charArray[^1] == ch;
    }

    public bool EndsWith(string text) => EndsWith(text.AsSpan());

    internal bool EndsWith(ReadOnlySpan<char> text) => Written.EndsWith(text);

    public void Clear()
    {
        _length = 0;
    }

    public void Dispose()
    {
        char[]? toReturn = _charArray;
        _charArray = null!;
        _length = 0;
        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    public override string ToString()
    {
        return new string(_charArray, 0, _length);
    }
}