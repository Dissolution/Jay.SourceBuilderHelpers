using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jay.SourceGen.Extensions;

public static class DeclarationSyntaxExtensions
{
    public static bool IsPartial(this ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    public static bool IsPartial(this StructDeclarationSyntax structDeclaration)
    {
        return structDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }
}
