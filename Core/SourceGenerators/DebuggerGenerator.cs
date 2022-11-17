using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Jay.SourceBuilderHelpers.SourceGenerators;

[Generator]
public sealed class DebuggerGenerator : ISourceGenerator, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif 
    }

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif 
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Do nothing
    }

       
}