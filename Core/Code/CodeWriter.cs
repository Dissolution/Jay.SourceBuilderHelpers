using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Jay.SourceGen.Text;

namespace Jay.SourceGen.Code;

/// <summary>
/// <see cref="CodeWriter"/> <see cref="Action"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public delegate void CWA(CodeWriter writer);

/// <summary>
/// An <c>Action&lt;<see cref="CodeWriter"/>, <typeparamref name="T"/>&gt;</c> delegate
/// </summary>
/// <typeparam name="T">The type of <paramref name="value"/> passed to the action</typeparam>
/// <param name="writer">An instance of a <see cref="CodeWriter"/></param>
/// <param name="value">A stateful <typeparamref name="T"/> value</param>
// ReSharper disable once InconsistentNaming
public delegate void CWA<in T>(CodeWriter writer, T value);

public partial class CodeWriter : IDisposable
{
    private const int MIN_CAPACITY = 1024;
    private const int MAX_CAPACITY = Constants.Pool_MaxCapacity;
    private readonly string _newLine = Environment.NewLine;
    private const string _defaultIndent = "  "; // two spaces

    private char[] _charArray;
    private int _length;
    private string _currentIndent = "";

    internal int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return _charArray.Length; }
    }

    internal Span<char> Available
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return _charArray.AsSpan(_length); }
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
        get { return _charArray.AsSpan(0, _length); }
    }

    /// <summary>
    /// Gets the current number of <see cref="char"/>acters written
    /// </summary>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return _length; }
    }

    public CodeWriter(int minCapacity = 1024)
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

    #region Write / WriteLine / NewLine

    public CodeWriter Write(char ch)
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

        return this;
    }

    public CodeWriter WriteLine(char ch)
    {
        return Write(ch).NewLine();
    }

    public CodeWriter Write(string? text)
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

        return this;
    }

    public CodeWriter WriteLine(string? text)
    {
        return Write(text).NewLine();
    }

    public CodeWriter Write(ReadOnlySpan<char> text)
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

        return this;
    }

    public CodeWriter WriteLine(ReadOnlySpan<char> text)
    {
        return Write(text).NewLine();
    }

    public CodeWriter Write<T>(T? value, string? format = null)
    {
        switch (value)
        {
            // CWA support for neat tricks
            case CWA codeWriterAction:
            {
                var currentIndent = CurrentLineIndent();
                return IndentBlock(currentIndent.ToString(), codeWriterAction);
            }
            case string str:
            {
                return CodeBlock(str);
            }
            case IFormattable formattable:
            {
                return Write(formattable.ToString(format, default));
            }
            case IEnumerable enumerable:
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return EnumerateDelimitWrite(format!, enumerable.Cast<object?>());
                }
                else
                {
                    return EnumerateWrite(enumerable.Cast<object?>());
                }
            }
            default:
            {
                return Write(value?.ToString());
            }
        }
    }

    public CodeWriter WriteLine<T>(T? value, string? format = null)
    {
        return Write<T>(value, format).NewLine();
    }

    /// <summary>
    /// Writes a new line (<c>Options.NewLine</c> + current indent)
    /// </summary>
    public CodeWriter NewLine()
    {
        return Write(_newLine).Write(_currentIndent);
    }

    /// <summary>
    /// Writes a new line (<c>Options.NewLine</c> + current indent)
    /// </summary>
    public CodeWriter NewLines(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Write(_newLine).Write(_currentIndent);
        }

        return this;
    }

    #endregion

    #region CodeBlock

    public CodeWriter CodeBlock(NonFormattableString nonFormattableString)
    {
        ReadOnlySpan<char> text = nonFormattableString.Text;
        int textLen = text.Length;

        ReadOnlySpan<char> newLine = Environment.NewLine.AsSpan();
        int newLineLen = newLine.Length;

        int start = 0;
        // if @ was used, there might be a leading newline, which we'll clean up
        if (text.StartsWith(newLine))
            start = newLineLen;

        // Slice up our text
        int index = start;

        int len;
        while (index < textLen)
        {
            if (text[index..].StartsWith(newLine))
            {
                len = index - start;
                if (len > 0)
                {
                    // Write this chunk, then a new line
                    Write(text.Slice(start, len));
                    NewLine();
                }

                // Skip ahead of this whitespace
                start = index + newLine.Length;
                index = start;
            }
            else
            {
                // Just text
                index++;
            }
        }

        // Anything left?
        len = index - start;
        if (len > 0)
        {
            // Write this chunk, then a new line
            Write(text.Slice(start, len));
            //NewLine();
        }

        return this;
    }

    private void WriteArgLine(ReadOnlySpan<char> line, object?[] args)
    {
        // Look for argument hole
        int lineEnd = line.Length - 1;
        int start = 0;
        int i = 0;
        char ch;
        while (i <= lineEnd)
        {
            ch = line[i];

            // Look for argument hole start
            if (ch == '{')
            {
                // Escaped?
                if (i == lineEnd || line[i + 1] == '{')
                {
                    // Write the pre-slice, skip the escaped char, continue scanning
                    Write(line[start..(i + 1)]);
                    i += 2;
                    start = i;
                    continue;
                }
                // This is a hole

                // Write the pre-slice
                Write(line[start..i]);
                i++;
                start = i;
                // Has to have a finish
                while (i <= lineEnd)
                {
                    ch = line[i];
                    if (ch == '}')
                    {
                        // Cannot be escaped
                        if (i < lineEnd && line[i + 1] == '}')
                            throw new ArgumentException();
                        break;
                    }

                    i++;
                }

                if (i > lineEnd)
                    throw new ArgumentException();

                // Hole (have to allocate)
                var argHole = line[start..i].ToString();
                if (int.TryParse(argHole, out int argIndex))
                {
                    if ((uint)argIndex >= args.Length)
                        throw new ArgumentException("Bad argument hole value", nameof(line));

                    Write<object?>(args[argIndex]);

                    i++;
                    start = i;
                    continue;
                }
                else
                {
                    // Account for format?
                    throw new InvalidOperationException();
                }
            }
            else if (ch == '}')
            {
                // Should not exist unless escaped
                if (i == lineEnd || line[i + 1] != '}')
                    throw new ArgumentException();
                // Write the pre-slice, skip the escaped char, continue scanning
                Write(line[start..(i + 1)]);
                i += 2;
                start = i;
                continue;
            }
            else
            {
                // Normal char, do nothing
                i++;
            }
        }

        // Post?
        if (start == 0)
        {
            Write(line);
        }
        else if (start > 0 && start < lineEnd)
        {
            Write(line[start..]);
        }
        else
        {
            // Already done
        }
    }

    public CodeWriter CodeBlock(FormattableString formattableString)
    {
        ReadOnlySpan<char> format = formattableString.Format.AsSpan();
        int formatLen = format.Length;
        object?[] formatArgs = formattableString.GetArguments();

        ReadOnlySpan<char> newLine = _newLine.AsSpan();
        int newLineLen = newLine.Length;

        int start = 0;
        // if @ was used, there might be a leading newline, which we'll clean up
        if (format.StartsWith(newLine))
            start = newLineLen;

        // Slice up our text
        int index = start;

        int len;
        while (index < formatLen)
        {
            // If we're at a newline, we split here
            if (format[index..].StartsWith(newLine))
            {
                len = index - start;
                if (len > 0)
                {
                    // Write this chunk, then a new line
                    WriteArgLine(format.Slice(start, len), formatArgs);
                    NewLine();
                }

                // Skip ahead of this whitespace
                start = index + newLine.Length;
                index = start;
            }
            else
            {
                // Just text
                index++;
            }
        }

        // Anything left?
        len = index - start;
        if (len > 0)
        {
            // Write this chunk, then a new line
            WriteArgLine(format.Slice(start, len), formatArgs);
            NewLine();
        }

        return this;
    }

    #endregion

    #region Fluent CS File

    /// <summary>
    /// Adds the `// &lt;auto-generated/&gt; ` line, optionally expanding it to include a <paramref name="comment"/>
    /// </summary>
    public CodeWriter AutoGeneratedHeader(string? comment = null)
    {
        if (comment is null)
        {
            return Write("// <auto-generated/>");
        }

        return this;

        /*
        var lines = comment.AsSpan().SplitLines();
        return WriteLine("// <auto-generated>")
            .IndentBlock("// ", ib =>
            {
                foreach (var line in lines)
                {
                    ib.WriteLine(comment.AsSpan()[line]);
                }
            })
            .WriteLine("// </auto-generated>");
        */
    }

    public CodeWriter Nullable(bool enable)
    {
        return Write("#nullable ")
            .Write(enable ? "enable" : "disable")
            .NewLine();
    }

    /// <summary>
    /// Writes a `using <paramref name="nameSpace"/>;` line
    /// </summary>
    public CodeWriter Using(string nameSpace)
    {
        ReadOnlySpan<char> ns = nameSpace.AsSpan();
        ns = ns.TrimStart("using ".AsSpan()).TrimEnd(';');
        return Write("using ").Write(ns).Write(';').NewLine();
    }

    /// <summary>
    /// Writes multiple <see cref="Using(string)"/> <paramref name="namespaces"/>
    /// </summary>
    public CodeWriter Using(params string[] namespaces)
    {
        foreach (var nameSpace in namespaces)
        {
            Using(nameSpace);
        }

        return this;
    }

    public CodeWriter Namespace(string? nameSpace)
    {
        if (!string.IsNullOrWhiteSpace(nameSpace))
        {
            return Write("namespace ").Write(nameSpace).WriteLine(';');
        }

        return this;
    }


    /// <summary>
    /// Writes the given <paramref name="comment"/> as a comment line / lines
    /// </summary>
    public CodeWriter Comment(string? comment)
    {
        /* Most of the time, this is probably a single line.
         * But we do want to watch out for newline characters to turn
         * this into a multi-line comment */
        var lines = comment.AsSpan().SplitLines();
        switch (lines.Count)
        {
            case 0:
                // Empty comment
                return WriteLine("// ");
            case 1:
                // Single line
                return Write("// ").WriteLine(comment);
            default:
                return Write("/* ").WriteLine(comment![lines[0]])
                    .IndentBlock(" * ", ib => ib.EnumerateLines(lines.Skip(1), (cw, line) => cw.WriteLine(comment[line])))
                    .WriteLine(" */");
        }
    }

    public CodeWriter Comment(string? comment, CommentType commentType)
    {
        var lines = comment.AsSpan().SplitLines();
        if (commentType == CommentType.SingleLine)
        {
            foreach (var line in lines)
            {
                Write("// ").WriteLine(comment![line]);
            }
        }
        else if (commentType == CommentType.XML)
        {
            foreach (var line in lines)
            {
                Write("/// ").WriteLine(line);
            }
        }
        else
        {
            return Write("/* ").WriteLine(comment![lines[0]])
                .IndentBlock(" * ", ib => ib.EnumerateLines(lines.Skip(1), (cw, line) => cw.WriteLine(comment[line])))
                .WriteLine(" */");
        }

        return this;
    }

    #endregion


    #region Blocks

    public CodeWriter BracketBlock(CWA bracketBlock, string indent = _defaultIndent)
    {
        return TrimEndWhiteSpace()
            .NewLine()
            .WriteLine('{')
            .IndentBlock(indent, bracketBlock)
            .EnsureOnNewLine()
            .Write('}');
    }

    public CodeWriter IndentBlock(string indent, CWA indentBlock)
    {
        if (IsOnNewLine())
        {
            Write(indent);
        }

        var oldIndent = _currentIndent;
        var newIndent = oldIndent + indent;
        _currentIndent = newIndent;
        indentBlock(this);
        _currentIndent = oldIndent;
        // Did we do a nl that we need to decrease?
        if (EndsWith(newIndent))
        {
            _length -= newIndent.Length;
            return Write(oldIndent);
        }

        return this;
    }

    #endregion


    #region Enumerate

    public CodeWriter Enumerate<T>(IEnumerable<T> values, CWA<T> perValue)
    {
        foreach (var value in values)
        {
            perValue(this, value);
        }

        return this;
    }

    public CodeWriter EnumerateWrite<T>(IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            Write<T>(item);
        }

        return this;
    }

    public CodeWriter EnumerateWriteLines<T>(IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            WriteLine<T>(item);
        }

        return this;
    }

    public CodeWriter EnumerateLines<T>(IEnumerable<T> enumerable, CWA<T> perValue)
    {
        foreach (var item in enumerable)
        {
            perValue(this, item);
            NewLine();
        }

        return this;
    }


    public CodeWriter EnumerateDelimitWrite<T>(string delimiter, IEnumerable<T> values)
    {
        using var e = values.GetEnumerator();
        if (!e.MoveNext()) return this;
        Write<T>(e.Current);
        while (e.MoveNext())
        {
            Write(delimiter);
            Write<T>(e.Current);
        }

        return this;
    }

    public CodeWriter EnumerateDelimit<T>(string delimiter, IEnumerable<T> values, CWA<T> perValue)
    {
        using var e = values.GetEnumerator();
        if (!e.MoveNext()) return this;
        perValue(this, e.Current);
        while (e.MoveNext())
        {
            Write(delimiter);
            perValue(this, e.Current);
        }

        return this;
    }

    #endregion


    #region Information about what has already been written

    internal bool IsOnWhiteSpace()
    {
        return _length == 0 || char.IsWhiteSpace(_charArray.AsSpan()[^1]);
    }

    internal bool IsOnNewLine()
    {
        return _length == 0 || Written.EndsWith(_newLine.AsSpan());
    }

    public bool StartsWith(char ch)
    {
        return _length > 0 && _charArray[0] == ch;
    }

    public bool StartsWith(string text)
    {
        return Written.StartsWith(text.AsSpan());
    }

    public bool StartsWith(ReadOnlySpan<char> text)
    {
        return Written.StartsWith(text);
    }

    public bool EndsWith(char ch)
    {
        return _length > 0 && _charArray.AsSpan()[^1] == ch;
    }

    public bool EndsWith(string text)
    {
        return Written.EndsWith(text.AsSpan());
    }

    public bool EndsWith(ReadOnlySpan<char> text)
    {
        return Written.EndsWith(text);
    }

    public ReadOnlySpan<char> CurrentLineIndent()
    {
        var lastNewLineIndex = Written.LastIndexOf(_newLine.AsSpan());
        if (lastNewLineIndex == -1)
            return default;
        lastNewLineIndex += _newLine.Length;
        var indent = Written[lastNewLineIndex..];
        return indent;
    }

    #endregion

    #region Formatting / Alignment
    public CodeWriter EnsureWhiteSpace()
    {
        if (!IsOnWhiteSpace())
            return Write(' ');
        return this;
    }

    /// <summary>
    /// Ensures that the writer is on the beginning of a new line
    /// </summary>
    public CodeWriter EnsureOnNewLine()
    {
        if (!IsOnNewLine())
            return NewLine();
        return this;
    }

    #endregion

    #region Trim

    public CodeWriter TrimEndWhiteSpace()
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
        return this;
    }

    #endregion


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
    public override bool Equals(object? obj)
    {
        throw new NotSupportedException();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
    {
        throw new NotSupportedException();
    }

    public override string ToString()
    {
        return new string(_charArray, 0, _length);
    }
}