using System;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
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

public sealed class CodeWriter : IDisposable
{
    private const int MIN_CAPACITY = 1024;
    private const int MAX_CAPACITY = Constants.Pool_MaxCapacity;
    private readonly string _defaultNewLine = Environment.NewLine;
    private const string _defaultIndent = "    "; // 4 spaces 

    private char[] _charArray;
    private int _length;
    private string _newLineIndent;

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
    }

    public CodeWriter(int minCapacity = 1024)
    {
        _charArray = ArrayPool<char>.Shared.Rent(minCapacity.Clamp(MIN_CAPACITY, MAX_CAPACITY));
        _newLineIndent = _defaultNewLine;
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
            case null:
            {
                return this;
            }
            // CWA support for neat tricks
            case CWA codeWriterAction:
            {
                var oldIndent = _newLineIndent;
                var currentIndent = CurrentNewLineIndent();
                _newLineIndent = currentIndent;
                codeWriterAction(this);
                _newLineIndent = oldIndent;
                return this;
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
        return Write(_newLineIndent);
    }

    /// <summary>
    /// Writes a new line (<c>Options.NewLine</c> + current indent)
    /// </summary>
    public CodeWriter NewLines(int count)
    {
        for (var i = 0; i < count; i++)
        {
            NewLine();
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
            if (text.Slice(index).StartsWith(newLine))
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

    internal CodeWriter WriteFormatChunk(ReadOnlySpan<char> format, ReadOnlySpan<object?> args)
    {
        // Undocumented exclusive limits on the range for Argument Hole Index
        const int IndexLimit = 1_000_000; // Note:            0 <= ArgIndex < IndexLimit

        // Repeatedly find the next hole and process it.
        int pos = 0;
        char ch;
        while (true)
        {
            // Skip until either the end of the input or the first unescaped opening brace, whichever comes first.
            // Along the way we need to also unescape escaped closing braces.
            while (true)
            {
                // Find the next brace.  If there isn't one, the remainder of the input is text to be appended, and we're done.
                if ((uint)pos >= (uint)format.Length)
                {
                    return this;
                }

                ReadOnlySpan<char> remainder = format.Slice(pos);
                int countUntilNextBrace = remainder.IndexOfAny('{', '}');
                if (countUntilNextBrace < 0)
                {
                    return Write(remainder);
                }

                // Append the text until the brace.
                Write(remainder.Slice(0, countUntilNextBrace));
                pos += countUntilNextBrace;

                // Get the brace.
                // It must be followed by another character, either a copy of itself in the case of being escaped,
                // or an arbitrary character that's part of the hole in the case of an opening brace.
                char brace = format[pos];
                ch = MoveNext(format, ref pos);
                if (brace == ch)
                {
                    Write(ch);
                    pos++;
                    continue;
                }

                // This wasn't an escape, so it must be an opening brace.
                if (brace != '{')
                {
                    ThrowFormatException(format, pos, "Missing opening brace");
                }

                // Proceed to parse the hole.
                break;
            }

            // We're now positioned just after the opening brace of an argument hole, which consists of
            // an opening brace, an index, and an optional format
            // preceded by a colon, with arbitrary amounts of spaces throughout.
            ReadOnlySpan<char> itemFormatSpan = default; // used if itemFormat is null

            // First up is the index parameter, which is of the form:
            //     at least on digit
            //     optional any number of spaces
            // We've already read the first digit into ch.
            Debug.Assert(format[pos - 1] == '{');
            Debug.Assert(ch != '{');
            int index = ch - '0';
            // Has to be between 0 and 9
            if ((uint)index >= 10u)
            {
                ThrowFormatException(format, pos, "Invalid character in index");
            }

            // Common case is a single digit index followed by a closing brace.  If it's not a closing brace,
            // proceed to finish parsing the full hole format.
            ch = MoveNext(format, ref pos);
            if (ch != '}')
            {
                // Continue consuming optional additional digits.
                while (IsAsciiDigit(ch) && index < IndexLimit)
                {
                    // Shift by power of 10
                    index = (index * 10) + (ch - '0');
                    ch = MoveNext(format, ref pos);
                }

                // Consume optional whitespace.
                while (ch == ' ')
                {
                    ch = MoveNext(format, ref pos);
                }

                // We do not support alignment
                if (ch == ',')
                {
                    ThrowFormatException(format, pos, "Alignment is not supported");
                }

                // The next character needs to either be a closing brace for the end of the hole,
                // or a colon indicating the start of the format.
                if (ch != '}')
                {
                    if (ch != ':')
                    {
                        // Unexpected character
                        ThrowFormatException(format, pos, "Unexpected character");
                    }

                    // Search for the closing brace; everything in between is the format,
                    // but opening braces aren't allowed.
                    int startingPos = pos;
                    while (true)
                    {
                        ch = MoveNext(format, ref pos);

                        if (ch == '}')
                        {
                            // Argument hole closed
                            break;
                        }

                        if (ch == '{')
                        {
                            // Braces inside the argument hole are not supported
                            ThrowFormatException(format, pos, "Braces inside the argument hole are not supported");
                        }
                    }

                    startingPos++;
                    itemFormatSpan = format.Slice(startingPos, pos - startingPos);
                }
            }

            // Construct the output for this arg hole.
            Debug.Assert(format[pos] == '}');
            pos++;

            if ((uint)index >= (uint)args.Length)
            {
                throw new FormatException($"Invalid Format: Argument '{index}' does not exist");
            }

            string? itemFormat = null;
            if (itemFormatSpan.Length > 0)
                itemFormat = itemFormatSpan.ToString();

            object? arg = args[index];

            // Write this arg
            Write<object?>(arg, itemFormat);

            // Continue parsing the rest of the format string.
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char MoveNext(ReadOnlySpan<char> format, ref int pos)
        {
            pos++;
            if ((uint)pos >= (uint)format.Length)
            {
                ThrowFormatException(format, pos, "Ran out of room");
            }

            return format[pos];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsAsciiDigit(char ch)
        {
            return (uint)(ch - '0') <= (uint)('9' - '0');
        }

        [DoesNotReturn]
        static void ThrowFormatException(ReadOnlySpan<char> format, int pos, string? details = null)
        {
            var message = new StringBuilder()
                .Append("Invalid Format at position ").Append(pos).AppendLine()
                .Append(
                    $"{format.SafeSlice(pos, -16).ToString()}→{format[pos]}←{format.SafeSlice(pos + 1, 16).ToString()}");
            if (details is not null)
            {
                message.AppendLine()
                    .Append("Details: ").Append(details);
            }

            throw new FormatException(message.ToString());
        }
    }

    public CodeWriter CodeBlock(FormattableString formattableString)
    {
        ReadOnlySpan<char> format = formattableString.Format.AsSpan();
        int formatLen = format.Length;
        object?[] formatArgs = formattableString.GetArguments();

        ReadOnlySpan<char> newLine = _defaultNewLine.AsSpan();
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
            if (format.Slice(index).StartsWith(newLine))
            {
                len = index - start;
                if (len > 0)
                {
                    // Write this chunk, then a new line
                    WriteFormatChunk(format.Slice(start, len), formatArgs);
                }
                NewLine();

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
            WriteFormatChunk(format.Slice(start, len), formatArgs);
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
            return WriteLine("// <auto-generated/>");
        }

        var lines = comment.AsSpan().SplitLines();
        return WriteLine("// <auto-generated>")
            .IndentBlock("// ", ib =>
            {
                foreach (var (start, length) in lines)
                {
                    ib.WriteLine(comment.AsSpan(start, length));
                }
            })
            .WriteLine("// </auto-generated>");
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
                {
                    using var e = lines.GetEnumerator();
                    e.MoveNext();
                    Write("/* ").WriteLine(comment.AsSpan(e.Current.start, e.Current.length));
                    while(e.MoveNext())
                    {
                        Write(" * ").WriteLine(comment.AsSpan(e.Current.start, e.Current.length));
                    }
                    return WriteLine(" */");
                }                
        }
    }

    public CodeWriter Comment(string? comment, CommentType commentType)
    {
        var lines = comment.AsSpan().SplitLines();
        if (commentType == CommentType.SingleLine)
        {
            foreach (var (start, length) in lines)
            {
                Write("// ").WriteLine(comment.AsSpan(start, length));
            }
        }
        else if (commentType == CommentType.XML)
        {
            foreach (var (start, length) in lines)
            {
                Write("/// ").WriteLine(comment.AsSpan(start, length));
            }
        }
        else
        {
            using var e = lines.GetEnumerator();
            e.MoveNext();
            Write("/* ").WriteLine(comment.AsSpan(e.Current.start, e.Current.length));
            while(e.MoveNext())
            {
                Write(" * ").WriteLine(comment.AsSpan(e.Current.start, e.Current.length));
            }
            return WriteLine(" */");
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
        var oldIndent = _newLineIndent;
        // We might be on a new line, but not yet indented
        if (CurrentNewLineIndent() == oldIndent)
        {
            Write(indent);
        }

        var newIndent = oldIndent + indent;
        _newLineIndent = newIndent;
        indentBlock(this);
        _newLineIndent = oldIndent;
        // Did we do a nl that we need to decrease?
        if (Written.EndsWith(newIndent.AsSpan()))
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

    internal string CurrentNewLineIndent()
    {
        var lastNewLineIndex = Written.LastIndexOf(_defaultNewLine.AsSpan());
        if (lastNewLineIndex == -1)
            return _defaultNewLine;
        return Written.Slice(lastNewLineIndex).ToString();
    }

    #endregion

    #region Formatting / Alignment

    /// <summary>
    /// Ensures that the writer is on the beginning of a new line
    /// </summary>
    public CodeWriter EnsureOnNewLine()
    {
        if (!Written.EndsWith(_newLineIndent.AsSpan()))
        {
            return NewLine();
        }

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