﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Jay.SourceGen.Code;
using Jay.SourceGen;

namespace Jay.EntityGen
{
    [Generator]
    public class EntityGenerator : IIncrementalGenerator
    {
        private static bool CouldBeEntityAttributeAsync(
            SyntaxNode syntaxNode,
            CancellationToken token)
        {
            if (syntaxNode is not AttributeSyntax attribute)
                return false;

            var name = ExtractName(attribute.Name);

            if (name is not (Code.EntityAttribute.BaseName or Code.EntityAttribute.Name)) return false;

            // "attribute.Parent" is "AttributeListSyntax"
            // "attribute.Parent.Parent" is a C# fragment the attributes are applied to
            var fragment = attribute.Parent?.Parent;
            return fragment switch
            {
                StructDeclarationSyntax structDeclaration => structDeclaration.IsPartial(),
                ClassDeclarationSyntax classDeclaration => classDeclaration.IsPartial(),
                _ => false
            };
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

        private static ITypeSymbol? GetEnumTypeOrNull(
            GeneratorSyntaxContext context,
            CancellationToken token)
        {
            var attributeSyntax = (AttributeSyntax)context.Node;

            // "attribute.Parent" is "AttributeListSyntax"
            Debug.Assert(attributeSyntax.Parent is AttributeListSyntax);

            // "attribute.Parent.Parent" is a C# fragment the attributes are applied to
            var fragment = attributeSyntax.Parent?.Parent;
            ITypeSymbol? typeSymbol;

            if (fragment is StructDeclarationSyntax structDeclaration)
            {
                typeSymbol = context.SemanticModel.GetDeclaredSymbol(structDeclaration) as ITypeSymbol;
            }
            else if (fragment is ClassDeclarationSyntax classDeclaration)
            {
                typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as ITypeSymbol;
            }
            else
            {
                typeSymbol = null;
            }

            if (IsOurAttribute(typeSymbol))
            {
                return typeSymbol;
            }

            return null;
        }

        private static bool IsOurAttribute(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null) return false;
            var attributes = typeSymbol.GetAttributes();
            if (attributes.IsEmpty) return false;
            return attributes.Any(attr =>
            {
                var attrClass = attr.AttributeClass;
                if (attrClass is null) return false;
                if (attrClass.Name != Code.EntityAttribute.Name) return false;
                var ns = attrClass.ContainingNamespace;
                if (!ns.IsGlobalNamespace) return false;
                return true;
            });
        }

        private static void GenerateCode(
            SourceProductionContext context,
            ImmutableArray<ITypeSymbol> enumerations)
        {
            if (enumerations.IsDefaultOrEmpty)
                return;

            foreach (var type in enumerations)
            {
                var code = GenerateCodeFor(type);
                var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
                    ? null
                    : $"{type.ContainingNamespace}.";

                context.AddSource($"{typeNamespace}{type.Name}.g.cs", code);
            }
        }

        private static string GenerateCodeFor(ITypeSymbol type)
        {
            var ns = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : type.ContainingNamespace.ToString();
            var name = type.Name;

            using var writer = new CodeWriter();
            writer
                .AutoGeneratedHeader()
                .NewLine()
                .Namespace(ns)
                .NewLine()
                .CodeBlock($$"""
                    partial class {{name}}
                    {
                        public override string ToString()
                        {
                            return $"Entity-Thing-{Guid.NewGuid()}";
                        }
                    }
                    """);
            string code = writer.ToString();
            Debugger.Break();
            return code;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG && ATTACH
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            // Add our main Attribute
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource($"{Code.EntityAttribute.Name}.g.cs",
                    SourceText.From(Code.EntityAttribute.Code, Encoding.UTF8));

                ctx.AddSource($"{Code.KeyAttribute.Name}.g.cs",
                    SourceText.From(Code.KeyAttribute.Code, Encoding.UTF8));
            });

            IncrementalValueProvider<ImmutableArray<ITypeSymbol>> enumTypes = context
                .SyntaxProvider
                .CreateSyntaxProvider(CouldBeEntityAttributeAsync, GetEnumTypeOrNull)
                .Where(type => type is not null)
                .Collect()!;

            context.RegisterSourceOutput(enumTypes, GenerateCode);
        }
    }
}