﻿using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Jay.SourceGen.Comparison;
using Jay.SourceGen.Text;

namespace Jay.EnumGen;

/*
public enum TestEnum
{
    Default,
    Alpha,
    Beta,
    Gamma,
    Delta,
}

[Flags]
public enum TestFlagsEnum
{
    Default = 0,
    Alpha = 1 << 0,
    Beta = 1 << 1,
    Gamma = 1 << 2,
    Delta = 1 << 3,
}

public static partial class TestEnumExtensions
{
    public static string Name(this TestEnum testEnum) =>
        testEnum switch
        {
            TestEnum.Default => nameof(TestEnum.Default),
            TestEnum.Alpha => nameof(TestEnum.Alpha),
            TestEnum.Beta => nameof(TestEnum.Beta),
            TestEnum.Gamma => nameof(TestEnum.Gamma),
            TestEnum.Delta => nameof(TestEnum.Delta),
            _ => testEnum.ToString() ?? "",
        };

    public static bool HasFlag(this TestFlagsEnum testFlagsEnum, TestFlagsEnum flag)
    {
        return (testFlagsEnum & flag) != default;
    }

    public static int GetSetBitCount(long lValue)
    {
        int iCount = 0;

        //Loop the value while there are still bits
        while (lValue != 0)
        {
            //Remove the end bit
            lValue &= (lValue - 1);

            //Increment the count
            iCount++;
        }

        //Return the count
        return iCount;
    }

    /*public static int FlagCount(this TestFlagsEnum testFlagsEnum)
    {
        return EnumExtensions.PopCount((long)testFlagsEnum);
    }#1#
}
*/


[Generator]
public class EnumToCodeGenerator : IIncrementalGenerator
{
    internal readonly struct EnumInfo
    {
        public readonly string Namespace;
        public readonly string FQN;
        public readonly string Name;
        public readonly bool HasFlags;
        public readonly List<string> Members;

        public EnumInfo(string ns, string fqn, string name, bool hasFlags, List<string> members)
        {
            Namespace = ns;
            FQN = fqn;
            Name = name;
            HasFlags = hasFlags;
            Members = members;
        }
    }

    private static string GetEnumVariableName(EnumInfo enumInfo)
    {
        var name = enumInfo.FQN.WithNaming(Naming.Camel);
        if (SyntaxFacts.IsValidIdentifier(name))
        {
            return "value";
        }
        return name;
    }


    private static void WriteExtensions(CodeWriter writer, EnumInfo enumInfo,
        Action<CodeWriter> addMethods)
    {
        writer.WriteAutoGeneratedHeader()
            .Nullable(true)
            .Namespace(enumInfo.Namespace)
            .WriteLine()
            .Write("public static partial class ")
            .Write(enumInfo.Name).WriteLine("Extensions")
            .BracketBlock(addMethods);
    }

    private static void AddNameExtensionMethod(CodeWriter writer, EnumInfo enumInfo)
    {
        var enumVariableName = GetEnumVariableName(enumInfo);
        writer.Write("public static string Name(this ")
            .Write(enumInfo.Name)
            .Write(' ')
            .Write(enumVariableName)
            .WriteLine(") =>")
            .IndentBlock(methodBlock =>
            {
                methodBlock.Write(enumVariableName).WriteLine(" switch")
                    .BracketBlock(switchBlock =>
                    {
                        foreach (string member in enumInfo.Members)
                        {
                            switchBlock.Write(enumInfo.Name).Write('.').Write(member)
                                .Write(" => nameof(")
                                .Write(enumInfo.Name).Write('.').Write(member)
                                .WriteLine("),");
                        }

                        switchBlock.Write("_ => ").Write(enumVariableName).WriteLine(".ToString() ?? \"\",");
                    }).WriteLine(';');
            }).WriteLine();
    }

    private static void AddFlagsExtensionsMethods(CodeWriter writer, EnumInfo enumInfo)
    {
        var enumVariableName = GetEnumVariableName(enumInfo);
        writer.WriteLine($"public static int FlagCount(this {enumInfo.Name} {enumVariableName})")
            .BracketBlock(methodBlock =>
            {
                methodBlock.WriteLine($"return EnumExtensions.PopCount((long){enumVariableName});");
            }).WriteLine();
    }

    /*
        public static int FlagCount(this TestFlagsEnum testFlagsEnum)
    {
        return EnumExtensions.PopCount((long)testFlagsEnum);
    }
     */


    private static void CreateExtensions(Compilation compilation,
        ImmutableArray<EnumDeclarationSyntax> enumDeclarations,
        SourceProductionContext context)
    {
        // Do we have any to create?
        if (enumDeclarations.IsDefaultOrEmpty) return;

        // Cleanup
        IEnumerable<EnumDeclarationSyntax> enums = enumDeclarations.Distinct();

        var enumInfos = GetEnumInfos(compilation, enums, context.CancellationToken);
        if (enumInfos.Count > 0)
        {
            foreach (var enumInfo in enumInfos)
            {
                using var writer = new CodeWriter();
                WriteExtensions(writer, enumInfo, methodWriter =>
                {
                    AddNameExtensionMethod(methodWriter, enumInfo);

                    if (enumInfo.HasFlags)
                    {
                        AddFlagsExtensionsMethods(methodWriter, enumInfo);
                    }
                });
                var code = writer.ToString();
                context.AddSource($"{enumInfo.Name}Extensions.g.cs", 
                    SourceText.From(code, Encoding.UTF8));
            }
        }
    }

    private static List<EnumInfo> GetEnumInfos(Compilation compilation,
        IEnumerable<EnumDeclarationSyntax> enumDeclarations,
        CancellationToken token)
    {
        var enumInfos = new List<EnumInfo>();

        // We're interested in [Flags]
        INamedTypeSymbol? systemFlagsAttribute = compilation.GetTypeByMetadataName("System.FlagsAttribute");

        // Process all enum declarations
        foreach (var enumDeclaration in enumDeclarations)
        {
            token.ThrowIfCancellationRequested();

            SemanticModel semanticModel = compilation.GetSemanticModel(enumDeclaration.SyntaxTree);
            if (ModelExtensions.GetDeclaredSymbol(semanticModel, enumDeclaration) is not INamedTypeSymbol enumSymbol)
            {
                Debug.WriteLine("Could not find EnumDeclaration as a NamedTypeSymbol in semanticModel");
                continue;
            }

            string enumName = enumSymbol.Name;
            string enumNamespace = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString();
            var ns = Jay.SourceGen.Extensions.GetNamespace(enumDeclaration);
            Debug.Assert(ns == enumNamespace);
            var hasFlags = false;

            // Check this enum's attributes for [Flags]
            foreach (AttributeData attributeData in enumSymbol.GetAttributes())
            {
                // If this is [Flags], record it
                if (SymbolEqualityComparer.Default.Equals(systemFlagsAttribute, attributeData.AttributeClass))
                {
                    hasFlags = true;
                    break;
                }
            }

            string fullyQualifiedName = enumSymbol.ToString();
            //string underlyingType = enumSymbol.EnumUnderlyingType?.ToString() ?? "int";

            var enumMembers = enumSymbol.GetMembers();
            var memberNames = new List<string>(enumMembers.Length);

            foreach (var member in enumMembers)
            {
                // Has to be a constant field (aka an enum member)
                if (member is not IFieldSymbol field || field.ConstantValue is null)
                {
                    continue;
                }

                string memberName = field.Name;
                memberNames.Add(memberName);
            }

            enumInfos.Add(new EnumInfo(enumNamespace, fullyQualifiedName, enumName, hasFlags, memberNames));
        }

        return enumInfos;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add raw code
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("EnumExtensions.g.cs",
                 SourceText.From(Code.EnumExtensions_Code, Encoding.UTF8));
        });

        // First cache: all Enum Declarations
        IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations
            = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, token) => node is EnumDeclarationSyntax,
                    transform: static (ctx, token) => ctx.Node as EnumDeclarationSyntax)
                .Where(static e => e is not null)!;

        // Second cache: All the enum declarations for this compilation
        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<EnumDeclarationSyntax> EnumDeclarations)> compilationEnumDeclarations
            = context.CompilationProvider.Combine(enumDeclarations.Collect());

        // Do the work
        context.RegisterSourceOutput(compilationEnumDeclarations,
            static (ctx, source) => CreateExtensions(source.Compilation, source.EnumDeclarations, ctx));
    }
}