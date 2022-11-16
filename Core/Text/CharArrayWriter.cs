using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Jay.SourceBuilderHelpers.Text;

public sealed class CharArrayWriter : IDisposable
{
    private static readonly ObjectPool<CharArrayWriter> _pool;

    static CharArrayWriter()
    {
        _pool = new ObjectPool<CharArrayWriter>(
            () => new CharArrayWriter(),
            writer => writer.Clear());
    }

    public static CharArrayWriter Rent() => _pool.Rent();
    public static void Return(CharArrayWriter writer) => _pool.Return(writer);

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

    private CharArrayWriter()
    {
        _charArray = new char[1024];
        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int additionalChars)
    {
        int newCapacity = Math.Min((Capacity + additionalChars) * 2, Constants.Array_MaxLength);
        var newArray = new char[newCapacity];
        Array.Copy(_charArray, newArray, _length);
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
        int len = text.Length;
        Debug.Assert(i + len > Capacity);
        Grow(len);
        TextHelper.CopyTo(text, _charArray, i, len);
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
            var chars = _charArray;
            if (i + len <= chars.Length)
            {
                TextHelper.CopyTo(text, chars, i, len);
                _length = i + len;
            }
            else
            {
                GrowWrite(text);
            }
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

    public bool StartsWith(string text)
    {
        return _length >= text.Length &&
            TextHelper.Equals(text, _charArray, 0);
    }

    public bool EndsWith(char ch)
    {
        return _length > 0 &&
            _charArray[Capacity-1] == ch;
    }

    public bool EndsWith(string text)
    {
        int right = Length - text.Length;
        return right >= 0 &&
            TextHelper.Equals(text, _charArray, right);
    }

    public void Clear()
    {
        _length = 0;
    }

    public void Dispose()
    {
        _pool.Return(this);
    }

    public override string ToString()
    {
        return new string(_charArray, 0, _length);
    }
}