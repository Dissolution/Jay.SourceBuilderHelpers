using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jay.SourceGen;

public static class Extensions
{
    public static int Clamp(this int value, int min, int max)
    {
        return value < min ? min : value > max ? max : value;
    }

    public static string GetNamespace(this SyntaxNode syntaxNode)
    {
        // determine the namespace the class is declared in, if any
        string nameSpace = string.Empty;
        SyntaxNode? potentialNamespaceParent = syntaxNode.Parent;
        while (potentialNamespaceParent != null &&
            potentialNamespaceParent is not NamespaceDeclarationSyntax
            && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                namespaceParent = parent;
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
            }
        }

        return nameSpace;
    }

    public static string? GetNamespace(this ITypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace;
        if (ns.IsGlobalNamespace)
            return null;
        return ns.ToString();
    }
}