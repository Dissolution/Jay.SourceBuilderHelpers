#define ATTACH

using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Jay.SourceGen.EnumCode;

[Generator]
internal class DebugGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        #if DEBUG && ATTACH
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
        #endif
    }
}