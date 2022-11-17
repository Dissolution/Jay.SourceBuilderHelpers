using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Jay.SourceBuilderHelpers.Text;

public sealed class CharArrayWriter : IDisposable
{
    private char[] _charArray;
    private int _length;

    public ref char this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index < Capacity)
            {
                return ref _charArray[index];
            }
            throw new IndexOutOfRangeException();
        }
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
        set => _length = Utils.Clamp(value, 0, Constants.Array_MaxLength);
    }

    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _charArray.Length;
    }

    internal Span<char> Written
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _charArray.AsSpan(0, _length);
    }

    internal Span<char> Available
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _charArray.AsSpan(_length);
    }

    public CharArrayWriter()
    {
        _charArray = ArrayPool<char>.Shared.Rent(1024);
        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int additionalChars)
    {
        int newCapacity = Math.Min((Capacity + additionalChars) * 2, Constants.Array_MaxLength);
        var newArray = ArrayPool<char>.Shared.Rent(newCapacity);
        TextHelper.CopyTo(Written, newArray);
        ArrayPool<char>.Shared.Return(_charArray);
        _charArray = newArray;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowWrite(char ch)
    {
        int i = _length;
        Debug.Assert(i + 1 > Capacity);
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
            int i = _length;
            int len = text.Length;
            var available = Available;
            if (i + len <= available.Length)
            {
                TextHelper.CopyTo(text.AsSpan(), available);
                _length = i + len;
            }
            else
            {
                GrowWrite(text);
            }
        }
    }

    internal void Write(ReadOnlySpan<char> text)
    {
        int i = _length;
        int len = text.Length;
        var available = Available;
        if (i + len <= available.Length)
        {
            TextHelper.CopyTo(text, available);
            _length = i + len;
        }
        else
        {
            GrowWrite(text);
        }
    }

    public void WriteSlice(string text, int offset)
    {
        Write(text.AsSpan(offset));
    }

    public void WriteSlice(string text, int offset, int length)
    {
        Write(text.AsSpan(offset, length));
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

    internal bool StartsWith(ReadOnlySpan<char> text) => Written.StartsWith(text);

    public bool EndsWith(char ch)
    {
        return _length > 0 &&
            _charArray[Capacity-1] == ch;
    }

    public bool EndsWith(string text) => EndsWith(text.AsSpan());

    internal bool EndsWith(ReadOnlySpan<char> text) => Written.EndsWith(text);

    public void Clear()
    {
        _length = 0;
    }

    public void Dispose()
    {
        var toReturn = _charArray;
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