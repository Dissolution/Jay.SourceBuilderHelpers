using Microsoft.CodeAnalysis;

namespace Jay.SourceGen.Diagnostics;

public static class Descriptors
{
    public static class Category
    {
        public const string TypeIssues = "TypeIssues";
    }


    public static readonly DiagnosticDescriptor TypeMustBePartial = new(
        id: "DIAG001",
        title: "Type must be partial",
        messageFormat: "The type '{0}' must be partial",
        category: Category.TypeIssues,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}