using Jay.SourceGen;

using System.Collections.Immutable;

#nullable enable

namespace Jay.EntityGen;

internal sealed class CompilationAndTypeDeclarationsComparer :
    IEqualityComparer<(Compilation Compilition, ImmutableArray<TypeDeclarationSyntax> TypeDeclarations)>,
    IEqualityComparer<ImmutableArray<TypeDeclarationSyntax>>,
    IEqualityComparer<TypeDeclarationSyntax>
{
    public bool Equals(TypeDeclarationSyntax? left, TypeDeclarationSyntax? right)
    {
        return string.Equals(left?.Identifier.Text, right?.Identifier.Text);
    }

    public bool Equals(ImmutableArray<TypeDeclarationSyntax> left, ImmutableArray<TypeDeclarationSyntax> right)
    {
        return left.SequenceEqual(right, (l, r) => Equals(l, r));
    }

    public bool Equals(
        (Compilation Compilition, ImmutableArray<TypeDeclarationSyntax> TypeDeclarations) left,
        (Compilation Compilition, ImmutableArray<TypeDeclarationSyntax> TypeDeclarations) right)
    {
        return Equals(left.TypeDeclarations, right.TypeDeclarations);
    }


    public int GetHashCode(TypeDeclarationSyntax? typeDeclarationSyntax)
    {
        if (typeDeclarationSyntax is null) return 0;
        return typeDeclarationSyntax.Identifier.Text.GetHashCode();
    }

    public int GetHashCode(ImmutableArray<TypeDeclarationSyntax> typeDeclarations)
    {
        return Hasher.GenerateHashCode(typeDeclarations, GetHashCode);
    }

    public int GetHashCode((Compilation Compilition, ImmutableArray<TypeDeclarationSyntax> TypeDeclarations) compilationAndTypeDeclarations)
    {
        return GetHashCode(compilationAndTypeDeclarations.TypeDeclarations);
    }
}