using CodeGen;

namespace Jay.SourceBuilderHelpers.Text;

/// <summary>
/// A fluent, reusable C# code writer
/// </summary>
public sealed class CodeWriter : IDisposable
{
    private readonly CharArrayWriter _writer;

    /// <summary>
    /// The current indent to apply after a NewLine operation
    /// </summary>
    private string _indent;

    /// <summary>
    /// The current <see cref="string"/> written with a <see cref="NL"/>
    /// </summary>
    public string NewLine => $"{Environment.NewLine}{_indent}";
    
    public CodeWriter()
    {
        _writer = CharArrayWriter.Rent();
        _indent = ""; // start with no indent
    }
    
    private bool IsOnNewLine()
    {
        return _writer.Length == 0 || _writer.EndsWith(NewLine);
    }


    /// <summary>
    /// Writes the given <paramref name="comment"/> as a comment line / lines
    /// </summary>
    public CodeWriter C(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return WL("// ");   // Empty comment
        }

        /* Most of the time, this is probably a single line.
         * But we do want to watch out for newline characters to turn
         * this into a multi-line comment */
        var lines = comment!.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
        return lines.Length switch
               {
                   0 => this,
                   1 => W("// ").WL(comment),
                   _ => W("/* ").D(c => c.NL().W(" * "), lines).NL().WL(" */")
               };
    }

    public CodeWriter NL() => W(NewLine);

    /// <summary>
    /// Writes <see cref="NewLine"/> the given <paramref name="count"/> times
    /// </summary>
    public CodeWriter NL(int count)
    {
        for (var i = 0; i < count; i++)
        {
            W(NewLine);
        }
        return this;
    }

    /// <summary>
    /// Ensures that the previous written characters are <see cref="NewLine"/>
    /// </summary>
    public CodeWriter ENL()
    {
        if (!IsOnNewLine())
            return NL();
        return this;
    }

    /// <summary>
    /// Writes the given <see cref="char"/>
    /// </summary>
    public CodeWriter W(char ch)
    {
        _writer.Write(ch);
        return this;
    }

    /// <summary>
    /// Writes the given <see cref="string"/>
    /// </summary>
    public CodeWriter W(string? text)
    {
        if (text is not null)
            _writer.Write(text);
        return this;
    }

    /// <summary>
    /// Writes the given <paramref name="value"/>
    /// </summary>
    public CodeWriter W<T>(T? value)
    {
        //_stringBuilder.Append((object?)value);
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

    public CodeWriter W(string text, Naming naming)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("", nameof(text));
        switch (naming)
        {
            case Naming.Field:
            {
                if (!text.StartsWith("_"))
                    W('_');
                return W(char.ToLower(text[0])).E(text.Skip(1));
            }
            case Naming.Camel:
            {
                return W(char.ToLower(text[0])).E(text.Skip(1));
            }
            case Naming.Pascal:
            {
                return W(char.ToUpper(text[0])).E(text.Skip(1));
            }
            case Naming.Default:
            default:
                return W(text);
        }
    }

    public CodeWriter WL(string? text)
    {
        return W(text).NL();
    }
    public CodeWriter WL<T>(T? value)
    {
        return W<T>(value).NL();
    }

    public CodeWriter T(Type type)
    {
        TypeFormatter.WriteCode(_writer, type);
        return this;
    }

    public CodeWriter T<T>() => this.T(typeof(T));

    public CodeWriter BB(Action<CodeWriter> bracketBlock)
    {
        return ENL().WL('{').IB(bracketBlock).ENL().W('}');
    }

    public CodeWriter IB(Action<CodeWriter> indentBlock) => IB("  ", indentBlock);

    public CodeWriter IB(string indent, Action<CodeWriter> indentBlock)
    {
        if (IsOnNewLine())
        {
            W(indent);
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
            W(oldIndent);
        }
        return this;
    }

    public CodeWriter E<T>(IEnumerable<T> values) => D("", values);
    public CodeWriter E<T>(IEnumerable<T> values, Action<CodeWriter, T> valueBlock) => D("", values, valueBlock);
    public CodeWriter D<T>(string delimiter, IEnumerable<T> values, Action<CodeWriter, T> valueBlock, bool delimitAfterLast = false)
        => D<T>(c => c.W(delimiter), values, valueBlock, delimitAfterLast);
    public CodeWriter D<T>(string delimiter, IEnumerable<T> values, bool delimitAfterLast = false)
        => D<T>(c => c.W(delimiter), values, delimitAfterLast);
    public CodeWriter D<T>(Action<CodeWriter> delimit, IEnumerable<T> values, bool delimitAfterLast = false)
        => D<T>(delimit, values, static (c, value) => c.W(value));

    public CodeWriter D<T>(
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

    public CodeWriter X(Action<CodeWriter> action)
    {
        action(this);
        return this;
    }

    public void Dispose()
    {
        _writer.Dispose();
    }

    public override string ToString()
    {
        return _writer.ToString();
    }
}