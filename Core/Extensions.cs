using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Jay.SourceGen.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

    public static string GetFQN(this ITypeSymbol typeSymbol)
    {
        var symbolDisplayFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        return typeSymbol.ToDisplayString(symbolDisplayFormat);
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

    public static bool IsPartial(this ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    public static bool IsPartial(this StructDeclarationSyntax structDeclaration)
    {
        return structDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    public static string GetVariableName(this ITypeSymbol typeSymbol)
    {
        string name = typeSymbol.Name.WithNaming(Naming.Camel);
        if (SyntaxFacts.IsValidIdentifier(name))
            return name;
        return $"@{name}";
    }

    public static bool CanBeNull(this ITypeSymbol typeSymbol)
    {
        return !typeSymbol.IsValueType;
    }

    public static IReadOnlyDictionary<string, object?> GetArgs(this AttributeData attributeData)
    {
        var args = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        var ctorArgs = attributeData.ConstructorArguments;
        if (ctorArgs.Length > 0)
        {
            var ctorParams = attributeData.AttributeConstructor?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty;
            Debug.Assert(ctorArgs.Length == ctorParams.Length);
            int count = ctorArgs.Length;
            for (var i = 0; i < count; i++)
            {
                string name = ctorParams[i].Name;
                Debug.Assert(args.ContainsKey(name) == false);
                object? value;
                var arg = ctorArgs[i];
                if (arg.Kind == TypedConstantKind.Array)
                    value = arg.Values;
                else
                    value = arg.Value;
                args[name] = value;
            }
        }

        var namedArgs = attributeData.NamedArguments;
        if (namedArgs.Length > 0)
        {
            int count = namedArgs.Length;
            for (var i = 0; i < count; i++)
            {
                var arg = namedArgs[i];
                string name = arg.Key;
                Debug.Assert(args.ContainsKey(name) == false);
                object? value;
                if (arg.Value.Kind == TypedConstantKind.Array)
                    value = arg.Value.Values;
                else
                    value = arg.Value.Value;
                args[name] = value;
            }
        }

        return args;
    }
}