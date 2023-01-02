using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Jay.SourceGen.Extensions;

namespace Jay.SourceGen.Text;

public sealed class TextBuilder : IDisposable
{
    private const int MIN_CAPACITY = 1024;
    private const int MAX_CAPACITY = 0X7FFFFFC7; // Array.MaxLength

    private char[] _charArray;
    private int _length;

    internal int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _charArray.Length;
    }

    internal Span<char> Available
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
            if ((uint)index < _length)
            {
                return ref _charArray[index];
            }
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and {_length - 1}");
        }
    }

    public Span<char> Written
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _charArray.AsSpan(0, _length);
    }

    /// <summary>
    /// Gets the current number of <see cref="char"/>acters written
    /// </summary>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _length = value.Clamp(0, _length);
    }

    public TextBuilder(int minCapacity = MIN_CAPACITY)
    {
        _charArray = ArrayPool<char>.Shared.Rent(minCapacity.Clamp(MIN_CAPACITY, MAX_CAPACITY));
        _length = 0;
    }

    #region Growth

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int additionalChars)
    {
        int newCapacity = ((Capacity + additionalChars) * 2).Clamp(MIN_CAPACITY, MAX_CAPACITY);
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

    #endregion


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(string? text)
    {
        if (text is not null)
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        string? text;
        if (value is IFormattable)
        {
            text = ((IFormattable)value).ToString(default, default);
        }
        else
        {
            text = value?.ToString();
        }
        Write(text);
    }

    public void WriteFormat<T>(T? value, string? format, IFormatProvider? provider = default)
    {
        string? text;
        if (value is IFormattable)
        {
            text = ((IFormattable)value).ToString(format, provider);
        }
        else
        {
            text = value?.ToString();
        }
        Write(text);
    }

    public void NewLine()
    {
        Write(Environment.NewLine);
    }

    /// <summary>
    /// Writes a new line (<c>Options.NewLine</c> + current indent)
    /// </summary>
    public void NewLines(int count)
    {
        for (var i = 0; i < count; i++)
        {
            NewLine();
        }
    }

    public void TrimEndWhiteSpace()
    {
        int i = _length - 1;
        for (; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(_charArray[i]))
            {
                break;
            }
        }

        // Length has to include the last char
        _length = i + 1;
    }

    public void Clear()
    {
        _length = 0;
    }

    /// <summary>
    /// Returns all resources back to the shared pool
    /// </summary>
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

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => throw new NotSupportedException();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => throw new NotSupportedException();

    public override string ToString()
    {
        return new string(_charArray, 0, _length);
    }
}
