/*using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Jay.SourceGen;
using Jay.SourceGen.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jay.EntityGen.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EntityGenAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(Descriptors.TypeMustBePartial);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (!EntityGenerator.IsOurAttribute(context.Symbol))
            return;

        var type = (INamedTypeSymbol)context.Symbol;

        foreach (var declaringSyntaxReference in type.DeclaringSyntaxReferences)
        {
            if (declaringSyntaxReference.GetSyntax()
                    is not ClassDeclarationSyntax classDeclaration ||
                classDeclaration.IsPartial())
                continue;

            var error = Diagnostic.Create(Descriptors.TypeMustBePartial,
                classDeclaration.Identifier.GetLocation(),
                type.Name);
            context.ReportDiagnostic(error);
        }
    }
}*/