using Jay.SourceGen.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jay.SourceGen.Extensions;

public static class TypeSymbolExtensions
{
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

    public static bool HasInterface<TInterface>(this ITypeSymbol type)
       where TInterface : class
    {
        var interfaceType = typeof(TInterface);
        if (!interfaceType.IsInterface)
            throw new ArgumentException("The generic type must be an Interface type", nameof(TInterface));
        var interfaceFQN = interfaceType.FullName;

        return type.AllInterfaces
            .Any(ti => ti.GetFQN() == interfaceFQN);
    }
}
