using System.Text;

using Microsoft.CodeAnalysis.Text;

namespace Jay.SourceGen;

public sealed record class CodeSource(string HintName, SourceText Text)
{
    public CodeSource(string hintName, string text)
        : this(hintName, SourceText.From(text, Encoding.UTF8))
    {

    }
}