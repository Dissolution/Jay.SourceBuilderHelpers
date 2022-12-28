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

    public static string? GetNamespace(this SyntaxNode syntaxNode)
    {
        // determine the namespace the class is declared in, if any
        string? nameSpace = null;
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

    public static string? ExtractName(this NameSyntax? name)
    {
        return name switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null
        };
    }

    public static string ToCode(this Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Private:
                return "private";
            case Accessibility.ProtectedOrInternal:
            case Accessibility.ProtectedAndInternal:
                return "protected internal";
            case Accessibility.Protected:
                return "protected";
            case Accessibility.Internal:
                return "internal";
            case Accessibility.Public:
                return "public";
            case Accessibility.NotApplicable:
            default:
                return "";
        }
    }
}