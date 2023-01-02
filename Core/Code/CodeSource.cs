using System.Text;

using Microsoft.CodeAnalysis.Text;

namespace Jay.SourceGen.Code;

public sealed record class CodeSource(string HintName, SourceText Text)
{
    public CodeSource(string hintName, string text)
        : this(hintName, SourceText.From(text, Encoding.UTF8))
    {

    }
}