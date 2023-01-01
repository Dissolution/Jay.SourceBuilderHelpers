using System.Collections.Immutable;
using System.Diagnostics;
using Jay.EntityGen.Attributes;
using Jay.SourceGen;
using Jay.EntityGen.CodeGen;

#nullable enable

namespace Jay.EntityGen;

[Generator]
public class EntityGenerator : IIncrementalGenerator
{
    private static bool CouldBeEntityAttributeAsync(SyntaxNode syntaxNode, CancellationToken _)
    {
        if (syntaxNode is not AttributeSyntax attribute)
            return false;

        var name = ExtractName(attribute.Name);

        return name is SourceNames.ClassAttributeName or SourceNames.ClassAttributeShortName;
    }

    private static string? ExtractName(NameSyntax? name)
    {
        return name switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null
        };
    }

    private static TypeDeclarationSyntax? GetTypeDeclarationOrNull(GeneratorSyntaxContext context, CancellationToken _)
    {
        Debug.Assert(context.Node is AttributeSyntax);
        var attributeSyntax = (AttributeSyntax)context.Node;

        // "attribute.Parent" is "AttributeListSyntax"
        Debug.Assert(attributeSyntax.Parent is AttributeListSyntax);

        // "attribute.Parent.Parent" is a C# fragment the attributes are applied to
        var fragment = attributeSyntax.Parent?.Parent;

        if (fragment is TypeDeclarationSyntax typeDeclarationSyntax)
        {
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

        // Get all the members
        ImmutableArray<ISymbol> typeMembers = typeSymbol.GetMembers();

        // Get all of them that have the Key attribute
        var members = new List<EntityMember>();

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
            else
            {
                continue;
            }

            var keyAttr = attributes.FirstOrDefault(attr =>
            {
                string? attrName = attr.AttributeClass?.Name;
                return attrName is SourceNames.PropAttributeName or SourceNames.PropAttributeShortName;
            });

            if (keyAttr is not null)
            {
                var attrArgs = keyAttr.GetArgs();

                KeyKind keyKind = KeyKind.None;
                if (attrArgs.TryGetValue("Kind", out object? kind))
                {
                    Debug.Assert(kind is not null);
                    Debug.Assert(Enum.IsDefined(typeof(KeyKind), kind));
                    keyKind = (KeyKind)kind;
                }

                if (keyKind != KeyKind.None)
                {
                    members.Add(new(
                        member.Name,
                        memberType,
                        keyKind));
                }
            }
        }

        var entityInfo = new EntityInfo(typeSymbol, members, true);

        yield return EquatableCodeSource.Generate(entityInfo);

        if (members.Count(m => m.Kind.HasFlag(KeyKind.Comparison)) == 1)
        {
            yield return ComparableCodeSource.Generate(entityInfo);
        }

        yield return FormattableCodeSource.Generate(entityInfo);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if ATTACH
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        //// Add our main Attribute
        //context.RegisterPostInitializationOutput(ctx =>
        //{
        //    ctx.AddSource($"{Code.EntityAttribute.Name}.g.cs",
        //        SourceText.From(Code.EntityAttribute.Code, Encoding.UTF8));

        //    ctx.AddSource($"EntityPropAttributes.g.cs",
        //        SourceText.From(Code.PropertyAttributes.Code, Encoding.UTF8));
        //});

        // First pass simple filter
        IncrementalValuesProvider<TypeDeclarationSyntax> enumDeclarations = context
            .SyntaxProvider
            .CreateSyntaxProvider(
                // select all of our attribute
                predicate: static (s, t) => CouldBeEntityAttributeAsync(s, t),
                // get the type they are declared upon
                transform: static (ctx, t) => GetTypeDeclarationOrNull(ctx, t))
            // Cleanup
            .Where(static m => m is not null)!;

        // Combine the selected enums with the `Compilation`
        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<TypeDeclarationSyntax> TypeDeclarations)> combined = context
            .CompilationProvider
            .Combine(enumDeclarations.Collect())
            .WithComparer(new CompilationAndTypeDeclarationsComparer());

        // Generate the output source
        context.RegisterSourceOutput(combined,
            static (spc, src) => AddCodeSources(spc, src.Compilation, src.TypeDeclarations));
    }
}
