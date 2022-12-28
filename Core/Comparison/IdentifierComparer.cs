using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jay.SourceGen.Comparison;

public class IdentifierComparer : IEqualityComparer<TypeDeclarationSyntax>
{
    public bool Equals(TypeDeclarationSyntax x, TypeDeclarationSyntax y)
    {
        return string.Equals(
            x.Identifier.Text,
            y.Identifier.Text);
    }

    public int GetHashCode(TypeDeclarationSyntax? obj)
    {
        if (obj is null) return 0;
        return obj.Identifier.Text.GetHashCode();
    }
}

public class IdentifierAndCompilationComparer
    : IEqualityComparer<(TypeDeclarationSyntax Node, Compilation compilation)>
{
    public bool Equals(
        (TypeDeclarationSyntax Node, Compilation compilation) x,
        (TypeDeclarationSyntax Node, Compilation compilation) y)
    {
        return string.Equals(
            x.Node.Identifier.Text, 
            y.Node.Identifier.Text);
    }

    public int GetHashCode((TypeDeclarationSyntax Node, Compilation compilation) obj)
    {
        return obj.Node.Identifier.Text.GetHashCode();
    }
}