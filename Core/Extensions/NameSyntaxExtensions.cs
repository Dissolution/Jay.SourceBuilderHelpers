using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jay.SourceGen.Extensions;

public static class NameSyntaxExtensions
{
    public static string? ExtractName(this NameSyntax? name)
    {
        return name switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null
        };
    }
}
