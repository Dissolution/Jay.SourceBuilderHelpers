using System.Collections.Immutable;
using System.Diagnostics;
using Jay.EntityGen.Attributes;
using Jay.EntityGen.CodeGen;
using Jay.SourceGen.Extensions;
using Jay.SourceGen.Code;

#nullable enable

namespace Jay.EntityGen;

public abstract class SyntaxIncrementalGenerator
{
    public static bool IsSyntaxNode<TSyntax>(SyntaxNode syntaxNode, string attributeName, CancellationToken _ = default)
        where TSyntax : MemberDeclarationSyntax
    {
        // Has to be our specific syntax
        if (syntaxNode is not TSyntax syntax)
        {
            return false;
        }

        // Has to be partial
        if (!syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return false;
        }

        // Has to have our attribute
        if (syntax.AttributeLists.Count == 0)
        {
            return false;
        }

        if (syntax.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(attr => attr.Name.IdentifierName() == attributeName))
        {
            return true;
        }

        return false;
    }
}


[Generator]
public class EntityGenerator : IIncrementalGenerator
{
    private static bool CouldBeEntityAttributeAsync(SyntaxNode syntaxNode, CancellationToken _)
    {
        if (syntaxNode is not AttributeSyntax attributeSyntax)
            return false;

        var name = attributeSyntax.Name.IdentifierName();

        return name is nameof(EntityAttribute) or "Entity";
    }

    private static TypeDeclarationSyntax? GetTypeDeclarationOrNull(GeneratorSyntaxContext context, CancellationToken _)
    {
        var attributeSyntax = (context.Node as AttributeSyntax);

        // "attribute.Parent" is "AttributeListSyntax"
        var attributeParent = (attributeSyntax?.Parent as AttributeListSyntax);
        
        // "attribute.Parent.Parent" is a C# fragment the attributes are applied to
        var fragment = attributeParent?.Parent;

        if (fragment is TypeDeclarationSyntax typeDeclarationSyntax)
        {
            if (!typeDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                Debugger.Break();

            return typeDeclarationSyntax;
        }

        // Did not find a type declaration (weird?)
        Debugger.Break();

        return null;
    }

    private static void AddCodeSources(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<TypeDeclarationSyntax> typeDeclarations)
    {
        if (typeDeclarations.IsDefaultOrEmpty) return;

        foreach (var typeDeclaration in typeDeclarations)
        {
            // Code!!!
            foreach (var codePart in GetCodeParts(context, compilation, typeDeclaration))
            {
                context.AddSource(codePart);
            }
        }
    }

    private static IEnumerable<CodeSource> GetCodeParts(
        SourceProductionContext context,
        Compilation compilation,
        TypeDeclarationSyntax typeDeclarationSyntax)
    {
        //  Get the semantic representation of the enum syntax
        SemanticModel semanticModel = compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
        INamedTypeSymbol typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclarationSyntax)!;
        if (typeSymbol is null) throw new InvalidOperationException();

        var isSealed = typeDeclarationSyntax.Modifiers.Any(SyntaxKind.SealedKeyword);

        // Get all the members
        ImmutableArray<ISymbol> typeMembers = typeSymbol.GetMembers();

        // Get all of them that have the Key attribute
        var members = new List<EntityMemberInfo>();

        // Scan for instances of our Key attribute
        foreach (ISymbol member in typeMembers)
        {
            ImmutableArray<AttributeData> attributes;
            ITypeSymbol memberType;

            if (member is IPropertySymbol property)
            {
                attributes = property.GetAttributes();
                memberType = property.Type;
            }
            else if (member is IFieldSymbol field)
            {
                attributes = field.GetAttributes();
                memberType = field.Type;
            }
            else if (member is IEventSymbol @event)
            {
                attributes = @event.GetAttributes();
                memberType = @event.Type;
            }
            else
            {
                continue;
            }

            var keyAttr = attributes.FirstOrDefault(attr =>
            {
                string? attrName = attr.AttributeClass?.Name;
                return attrName is nameof(KeyAttribute) or "Key";
            });

            if (keyAttr is not null)
            {
                var attrArgs = keyAttr.GetArgs();

                KeyKind keyKind = KeyKind.None;
                if (attrArgs.TryGetValue("Kind", out object? kind))
                {
                    if (kind is null || !Enum.IsDefined(typeof(KeyKind), kind))
                    {
                        throw new InvalidOperationException();
                    }
                    keyKind = (KeyKind)kind!;
                }

                if (keyKind != KeyKind.None)
                {
                    members.Add(new(member.Name, memberType, keyKind));
                }
            }
        }

        EntityInfo entityInfo = new EntityInfo
        {
            Type = typeSymbol,
            Members = members,
            IsSealed = isSealed,
        };

        if (CodeSources.GenerateDeconstruct(entityInfo, out var deconstructSource))
        {
            yield return deconstructSource!;
        }

        if (CodeSources.GenerateEquality(entityInfo, out var equalitySource))
        {
            yield return equalitySource!;
        }

        if (CodeSources.GenerateComparison(entityInfo, out var comparisonSource))
        {
            yield return comparisonSource!;
        }

        if (CodeSources.GenerateFormattable(entityInfo, out var formattableSource))
        {
            yield return formattableSource!;
        }

        if (CodeSources.GenerateDisposable(entityInfo, out var disposableSource))
        {
            yield return disposableSource!;
        }
    }


    /// <inheritdoc cref="IIncrementalGenerator"/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if ATTACH
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        // First pass simple filter
        IncrementalValuesProvider<TypeDeclarationSyntax> enumDeclarations = context
            .SyntaxProvider
            .CreateSyntaxProvider(
                // select all of our attribute
                predicate: static (syntaxNode, token) => CouldBeEntityAttributeAsync(syntaxNode, token),
                // get the type they are declared upon
                transform: static (generatorSyntaxContext, token) => GetTypeDeclarationOrNull(generatorSyntaxContext, token))
            // Cleanup
            .Where(static m => m is not null)!;

        // Combine the selected enums with the `Compilation`
        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<TypeDeclarationSyntax> TypeDeclarations)> combined = context
            .CompilationProvider
            .Combine(enumDeclarations.Collect())
            .WithComparer(new CompilationAndTypeDeclarationsComparer());

        // Generate the output source
        context.RegisterSourceOutput(combined,
            static (sourceProductionContext, tuple) => AddCodeSources(sourceProductionContext, tuple.Compilation, tuple.TypeDeclarations));
    }
}
