//#define ATTACH

using System.Diagnostics;

namespace Jay.EnumGen;

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