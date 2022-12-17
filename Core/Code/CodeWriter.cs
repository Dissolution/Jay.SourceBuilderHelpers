using System.Collections;
using System.ComponentModel;
using Jay.SourceGen.Text;

// Purely for performance reasons
// ReSharper disable MergeCastWithTypeCheck

namespace Jay.SourceGen.Code;

public partial class CodeWriter
{
    public readonly struct NonFormattableString
    {
        //public static implicit operator string(NonFormattableString nfs) => nfs._str;
        public static implicit operator NonFormattableString(string str) => new NonFormattableString(str);
        public static implicit operator NonFormattableString(FormattableString _) => throw new InvalidOperationException();

        internal readonly string _str;

        private NonFormattableString(string str)
        {
            _str = str;
        }

        public override string ToString()
        {
            return _str;
        }
    }

    private static int GetFirstIndex(string str, char ch, int start = 0)
    {
        int i;
        int endIndex = str.Length - 1;
        while (true)
        {
            i = str.IndexOf(ch, start);
            if (i == -1) return -1;
            if (i == endIndex || str[i + 1] != ch)
                return i;
            start = i;
        }
    }

    public CodeWriter Append(NonFormattableString text)
    {
        return ParseString(text._str);
    }

    public CodeWriter Append(FormattableString formattableString)
    {
        var argCount = formattableString.ArgumentCount;
        if (argCount == 0)
        {
            return Parse(formattableString.Format);
        }

        string format = formattableString.Format;
        var lines = format.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            // Does this line contain an argument hole?
            int start = 0;
            int i;
            while ((i = GetFirstIndex(line, '{', start)) >= 0)
            {
                var s = i + 1;
                var e = GetFirstIndex(line, '}', s);
                if (e < 0) throw new ArgumentException("Missing argument hole close", nameof(formattableString));

                var pre = line.Substring(start, i - start);
                _writer.Write(pre);

                var argHole = line.Substring(s, e - s);
                if (int.TryParse(argHole, out int argIndex))
                {
                    if ((uint)argIndex >= argCount)
                        throw new ArgumentException("Bad argument hole value", nameof(formattableString));
                    object? arg = formattableString.GetArgument(argIndex);
                    this.WriteCode<object?>(arg);
                    i = e;
                    start = e + 1;
                    continue;
                }
                else
                {
                    // Account for format?
                    throw new InvalidOperationException();
                }
            }

            if (start > 0)
            {
                var post = line.Substring(start);
                _writer.Write(post);
            }
            else
            {
                _writer.Write(line);
            }
        }

        return this;
    }

    private CodeWriter ParseString(string? text)
    {
        if (text is null) return this;
        // Cleanup leading NewLine from `@` usage
        if (text.StartsWith(_defaultNewLine))
            text = text[_defaultNewLine.Length..];
        // Support for `@` and newlines
        var lines = TextHelper.Split(text, _defaultNewLine);
        int len = lines.Length;
        switch (len)
        {
            case 0:
                break;
            case 1:
                _writer.Write(lines[0]);
                break;
            default:
            {
                for (var i = 0; i < len; i++)
                {
                    AppendLine();
                    _writer.Write(lines[i]);
                }
                break;
            }
        }
        return this;
    }

   


    public CodeWriter AppendValue<T>(T? value)
    {
        switch (value)
        {
            case string str:
                return ParseString(str);
            case IFormattable:
                return ParseString(((IFormattable)value).ToString(default, default));
            case IEnumerable enumerable:
                return AppendDelimit<object?>(null, enumerable.Cast<object?>());
            default:
                return this.WriteCode<T>(value);
        }
    }

   
    public CodeWriter AppendFormat<T>(T? value, string? format)
    {
        switch (value)
        {
            case string str:
                return ParseString(str);
            case IFormattable:
                return ParseString(((IFormattable)value).ToString(format, default));
            case IEnumerable enumerable:
                return AppendDelimit<object?>(format, enumerable.Cast<object?>());
            default:
                return this.WriteCode<T>(value);
        }
    }

    public CodeWriter AppendLine() => NewLine();

    public CodeWriter AppendDelimit<T>(string? delimiter, IEnumerable<T> values)
    {
        if (string.IsNullOrEmpty(delimiter))
        {
            foreach (var value in values)
            {
                AppendValue<T>(value);
            }
        }
        else
        {
            using var e = values.GetEnumerator();
            if (!e.MoveNext()) return this;
            AppendValue<T>(e.Current);
            while (e.MoveNext())
            {
                AppendValue(delimiter);
                AppendValue<T>(e.Current);
            }
        }
        return this;
    }


}


/// <summary>
/// A fluent, reusable C# code writer
/// </summary>
public sealed partial class CodeWriter : IDisposable
{
    private readonly CharArrayWriter _writer;

    private readonly string _defaultNewLine;
    private readonly string _defaultIndent;
    private readonly bool _useJavaStyleBraces;


    private string _indent;
    private string _newLine;


    public CodeWriter()
        : this(CodeWriterOptions.Default) { }

    public CodeWriter(CodeWriterOptions options)
    {
        _defaultNewLine = options.NewLine;
        _defaultIndent = options.Indent;
        _useJavaStyleBraces = options.UseJavaStyleBraces;

        _writer = new CharArrayWriter();
        _indent = ""; // start with no indent
        _newLine = _defaultNewLine;
    }

    private bool IsOnWhiteSpace()
    {
        return _writer.Length == 0 || char.IsWhiteSpace(_writer[^1]);
    }

    private bool IsOnNewLine()
    {
        return _writer.Length == 0 || _writer.EndsWith(_newLine);
    }

    /// <summary>
    /// Adds the `// &lt;auto-generated/&gt; ` line, optionally expanding it to include a <paramref name="comment"/>
    /// </summary>
    public CodeWriter WriteAutoGeneratedHeader(string? comment = null)
    {
        if (comment is null)
        {
            return WriteLine("// <auto-generated/> ");
        }

        var commentLines = TextHelper.Split(comment, _defaultNewLine);
        return WriteLine("// <auto-generated>")
            .IndentBlock("// ", ic =>
            {
                ic.WriteLines(commentLines, static (c, s) => c._writer.Write(s));
            })
            .EnsureOnNewLine()
            .WriteLine("// </auto-generated>");
    }

    public CodeWriter Nullable(bool enable)
    {
        return Write("#nullable ")
            .Write(enable ? "enable" : "disable")
            .AppendLine();
    }

    /// <summary>
    /// Writes a `using <paramref name="nameSpace"/>;` line
    /// </summary>
    public CodeWriter Using(string nameSpace)
    {
        ReadOnlySpan<char> ns = nameSpace.AsSpan();
        ns = ns.TrimStart("using ".AsSpan()).TrimEnd(';');
        _writer.Write("using ");
        _writer.Write(ns);
        _writer.Write(';');
        return NewLine();
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
        var lines = TextHelper.Split(comment, _defaultNewLine);
        switch (lines.Length)
        {
            case 0:
                return WriteLine("// "); // Empty comment
            case 1:
                return Write("// ").WriteLine(comment);
            default:
                return Write("/* ").WriteLine(lines[0])
                    .IndentBlock(" * ", ic =>
                    {
                        ic.WriteLines(lines.Skip(1), static (c, s) => c._writer.Write(s));
                    })
                    .EnsureOnNewLine()
                    .WriteLine(" */");
        }
    }

    public CodeWriter Comment(string? comment, CommentType commentType)
    {
        var lines = TextHelper.Split(comment, _defaultNewLine);
        if (commentType == CommentType.SingleLine)
        {
            foreach (var line in lines)
            {
                Write("// ").WriteLine(line);
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
            Write("/* ").WriteLine(lines[0])
                .IndentBlock(" * ", ic =>
                {
                    ic.WriteLines(lines.Skip(1), static (c, s) => c._writer.Write(s));
                })
                .EnsureOnNewLine()
                .WriteLine(" */");
        }

        return this;
    }

    /// <summary>
    /// Writes a new line (<c>Options.NewLine</c> + current indent)
    /// </summary>
    public CodeWriter NewLine()
    {
        _writer.Write(_defaultNewLine);
        _writer.Write(_indent);
        return this;
    }

    /// <summary>
    /// Writes a new line (<c>Options.NewLine</c> + current indent)
    /// </summary>
    public CodeWriter NewLine(int count)
    {
        var nl = _defaultNewLine;
        for (var i = 0; i < count; i++)
        {
            _writer.Write(nl);
            _writer.Write(_indent);
        }

        return this;
    }

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

    /// <summary>
    /// Writes the given <see cref="char"/>
    /// </summary>
    public CodeWriter Write(char ch)
    {
        _writer.Write(ch);
        return this;
    }

    /// <summary>
    /// Writes the given <c>ReadOnlySpan&lt;char&gt;</c>
    /// </summary>
    public CodeWriter Write(ReadOnlySpan<char> text)
    {
        _writer.Write(text);
        return this;
    }

    /// <summary>
    /// Writes the given <see cref="string"/>
    /// </summary>
    public CodeWriter Write(string? text)
    {
        _writer.Write(text);
        return this;
    }

    public CodeWriter Parse(string? text)
    {
        if (text is null) return this;
        // Cleanup leading NewLine from `@` usage
        if (text.StartsWith(_defaultNewLine))
            text = text[_defaultNewLine.Length..];
        // Support for `@` and newlines
        var lines = TextHelper.Split(text, _defaultNewLine);
        if (lines.Length == 0) return this;
        if (lines.Length == 1)
        {
            _writer.Write(lines[0]);
            return this;
        }

        return WriteLines(lines, static (c, s) => c._writer.Write(s));
    }

    /// <summary>
    /// Writes the given <paramref name="value"/>
    /// </summary>
    public CodeWriter Write<T>(T? value)
    {
        if (value is IFormattable)
        {
            _writer.Write(((IFormattable)value).ToString(default, default));
        }
        else
        {
            _writer.Write(value?.ToString());
        }

        return this;
    }

    public CodeWriter Format(FormattableString format)
    {
        var fmt = format.Format;
        var argCount = format.ArgumentCount;
        if (argCount == 0)
        {
            return Parse(fmt);
        }
        if (argCount == 1)
        {
            var arg = format.GetArgument(0).ToCode();
            return Parse(string.Format(fmt, arg));
        }
        if (argCount == 2)
        {
            var arg0 = format.GetArgument(0).ToCode();
            var arg1 = format.GetArgument(1).ToCode();
            return Parse(string.Format(fmt, arg0, arg1));
        }
        if (argCount == 3)
        {
            var arg0 = format.GetArgument(0).ToCode();
            var arg1 = format.GetArgument(1).ToCode();
            var arg2 = format.GetArgument(2).ToCode();
            return Parse(string.Format(fmt, arg0, arg1, arg2));
        }

        var args = format.GetArguments();
        var newArgs = new object[argCount];
        for (var i = 0; i < argCount; i++)
        {
            newArgs[i] = args.ToCode();
        }
        return Parse(string.Format(fmt, newArgs));
    }

    public CodeWriter Format<T>(T? value, string? format, IFormatProvider? provider = null)
    {
        if (value is IFormattable)
        {
            _writer.Write(((IFormattable)value).ToString(format, provider));
        }
        else
        {
            _writer.Write(value?.ToString());
        }

        return this;
    }

    public CodeWriter Name(string memberName, Naming naming)
    {
        // Also ensures that Length > 0
        if (string.IsNullOrWhiteSpace(memberName))
            throw new ArgumentException("You must pass a valid Member name", nameof(memberName));
        if (naming is Naming.Default or Naming.Lower or Naming.Upper or Naming.Title or Naming.Snake)
        {
            return Write(memberName.WithNaming(naming));
        }

        if (naming == Naming.Field && !memberName.StartsWith("_"))
            _writer.Write('_');

        switch (naming)
        {
            case Naming.Field or Naming.Camel:
                _writer.Write(char.ToLower(memberName[0]));
                break;
            case Naming.Pascal:
                _writer.Write(char.ToUpper(memberName[0]));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(naming));
        }

        _writer.Write(memberName.AsSpan(1));
        return this;
    }



    public CodeWriter WriteLine(string? text)
    {
        return Write(text).NewLine();
    }

    public CodeWriter WriteLine<T>(T? value)
    {
        return Write<T>(value).NewLine();
    }

    public CodeWriter BracketBlock(Action<CodeWriter> bracketBlock)
    {
        DeleteWhiteSpace();

        if (_useJavaStyleBraces)
        {
            Write(' ');
        }
        else
        {
            NewLine();
        }

        return WriteLine('{')
            .IndentBlock(bracketBlock)
            .EnsureOnNewLine()
            .Write('}');
    }

    public CodeWriter IndentBlock(Action<CodeWriter> indentBlock) => IndentBlock(_defaultIndent, indentBlock);

    public CodeWriter IndentBlock(string indent, Action<CodeWriter> indentBlock)
    {
        if (IsOnNewLine())
        {
            Write(indent);
        }

        var oldIndent = _indent;
        var newIndent = $"{oldIndent}{indent}";
        _indent = newIndent;
        indentBlock(this);
        _indent = oldIndent;

        // Did we do a nl that we need to decrease?
        if (_writer.EndsWith(newIndent))
        {
            _writer.Length -= newIndent.Length;
            Write(oldIndent);
        }

        return this;
    }

    public CodeWriter Enumerate<T>(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            Write<T>(value);
        }

        return this;
    }

    public CodeWriter Enumerate<T>(IEnumerable<T> values, Action<CodeWriter, T> valueBlock)
    {
        foreach (var value in values)
        {
            valueBlock(this, value);
        }

        return this;
    }

    public CodeWriter WriteLines<T>(IEnumerable<T> values, Action<CodeWriter, T> valueBlock)
        => Delimited<T>(static c => c.NewLine(), values, valueBlock);

    public CodeWriter WriteLines<T>(IEnumerable<T> values)
        => Delimited<T>(static c => c.NewLine(), values, static (c, v) => c.Write<T>(v));

    public CodeWriter Delimited<T>(string delimiter, IEnumerable<T> values, bool delimitAfterLast = false)
        => Delimited<T>(delimiter, values, static (c, v) => c.Write<T>(v), delimitAfterLast);

    public CodeWriter Delimited<T>(string delimiter, IEnumerable<T> values, Action<CodeWriter, T> valueBlock,
        bool delimitAfterLast = false)
    {
        // Check for pass of NewLine
        if (delimiter == _defaultNewLine)
            return Delimited<T>(c => c.NewLine(), values, valueBlock, delimitAfterLast);
        return Delimited<T>(c => c.Write(delimiter), values, valueBlock, delimitAfterLast);
    }

    public CodeWriter Delimited<T>(Action<CodeWriter> delimit, IEnumerable<T> values, bool delimitAfterLast = false)
        => Delimited<T>(delimit, values, static (c, value) => c.Write(value), delimitAfterLast);

    public CodeWriter Delimited<T>(
        Action<CodeWriter> delimit,
        IEnumerable<T> values,
        Action<CodeWriter, T> valueBlock,
        bool delimitAfterLast = false)
    {
        if (delimitAfterLast)
        {
            foreach (var value in values)
            {
                valueBlock(this, value);
                delimit(this);
            }
        }
        else
        {
            using var e = values.GetEnumerator();
            if (!e.MoveNext()) return this;
            valueBlock(this, e.Current);
            while (e.MoveNext())
            {
                delimit(this);
                valueBlock(this, e.Current);
            }
        }

        return this;
    }

    public CodeWriter Execute(Action<CodeWriter> action)
    {
        action(this);
        return this;
    }


    public CodeWriter DeleteWhiteSpace()
    {
        var writer = _writer;
        int i = writer.Length - 1;
        for (; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(writer[i]))
            {
                break;
            }
        }

        // Length has to include the last char
        writer.Length = i + 1;
        return this;
    }


    public void Dispose()
    {
        _writer.Dispose();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => throw new NotSupportedException();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => throw new NotSupportedException();

    public override string ToString()
    {
        return _writer.ToString();
    }
}