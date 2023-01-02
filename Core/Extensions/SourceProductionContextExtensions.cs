using Jay.SourceGen.Code;
using Microsoft.CodeAnalysis;

namespace Jay.SourceGen.Extensions;

public static class SourceProductionContextExtensions
{
    public static void AddSource(this SourceProductionContext context,
        CodeSource codeSource)
    {
        context.AddSource(codeSource.HintName, codeSource.Text);
    }
}