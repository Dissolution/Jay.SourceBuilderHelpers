﻿using System.ComponentModel;

// Purely for performance reasons
// ReSharper disable MergeCastWithTypeCheck

namespace Jay.SourceBuilderHelpers.Text;

/// <summary>
/// A fluent, reusable C# code writer
/// </summary>
public sealed class CodeWriter : IDisposable
{
    private readonly CharArrayWriter _writer;

    /// <summary>
    /// The current indent to write after <see cref="NewLine()"/> operations
    /// </summary>
    private string _indent;

    public CodeWriterOptions Options { get; }

    public CodeWriter(CodeWriterOptions? options = null)
    {
        this.Options = options ?? new();
        _writer = new CharArrayWriter();
        _indent = ""; // start with no indent
    }

    private bool IsWhiteSpace()
    {
        int len = _writer.Length;
        return len == 0 || char.IsWhiteSpace(_writer[^1]);
    }

    private bool IsOnNewLine()
    {
        return _writer.Length == 0 || _writer.EndsWith(Options.NewLine + _indent);
    }

    public CodeWriter Using(string @namespace)
    {
        ReadOnlySpan<char> ns = @namespace.AsSpan();
        ns = ns.TrimStart("using ".AsSpan()).TrimEnd(';');
        _writer.Write("using ");
        _writer.Write(ns);
        _writer.Write(';');
        return NewLine();
    }

    public CodeWriter Using(params string[] namespaces)
    {
        foreach (var @namespace in namespaces)
        {
            Using(@namespace);
        }
        return this;
    }

    public CodeWriter WriteAutoGeneratedHeader(string? comment = null)
    {
        if (comment is null)
        {
            return WriteLine("// <auto-generated/> ");
        }

        var commentLines = TextHelper.Split(comment, Options.NewLine);
        return WriteLine("// <auto-generated>")
            .IndentBlock("// ", ic =>
            {
                ic.WriteLines(commentLines, static (c, s) => c._writer.Write(s));
            })
            .EnsureOnNewLine()
            .WriteLine("// </auto-generated>");
    }

    /// <summary>
    /// Writes the given <paramref name="comment"/> as a comment line / lines
    /// </summary>
    public CodeWriter Comment(string? comment)
    {
        /* Most of the time, this is probably a single line.
         * But we do want to watch out for newline characters to turn
         * this into a multi-line comment */
        var lines = TextHelper.Split(comment, Options.NewLine);
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

    /// <summary>
    /// Writes a new line (<c>Options.NewLine</c> + current indent)
    /// </summary>
    public CodeWriter NewLine()
    {
        _writer.Write(Options.NewLine);
        _writer.Write(_indent);
        return this;
    }

    /// <summary>
    /// Writes a new line (<c>Options.NewLine</c> + current indent)
    /// </summary>
    public CodeWriter NewLine(int count)
    {
        var nl = Options.NewLine;
        for (var i = 0; i < count; i++)
        {
            _writer.Write(nl);
            _writer.Write(_indent);
        }

        return this;
    }

    public CodeWriter EnsureWhiteSpace()
    {
        if (!IsWhiteSpace())
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
        if (text.StartsWith(Options.NewLine))
            text = text[Options.NewLine.Length..];
        // Support for `@` and newlines
        var lines = TextHelper.Split(text, Options.NewLine);
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
        if (naming == Naming.Default) return Write(memberName);
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

    public CodeWriter WriteLine() => NewLine();

    public CodeWriter WriteLine(string? text)
    {
        return Write(text).NewLine();
    }

    public CodeWriter WriteLine<T>(T? value)
    {
        return Write<T>(value).NewLine();
    }

    public CodeWriter Type(Type type)
    {
        CodeFormatter.WriteCodeTo(type, _writer);
        return this;
    }

    public CodeWriter Type<T>() => this.Type(typeof(T));

    public CodeWriter BracketBlock(Action<CodeWriter> bracketBlock)
    {
        DeleteWhiteSpace();

        if (Options.UseJavaStyleBraces)
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

    public CodeWriter IndentBlock(Action<CodeWriter> indentBlock) => IndentBlock(Options.Indent, indentBlock);

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
        if (delimiter == Options.NewLine)
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

    [Obsolete("Not Supported", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => throw new NotSupportedException();

    [Obsolete("Not Supported", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => throw new NotSupportedException();

    public override string ToString()
    {
        return _writer.ToString();
    }
}