//#define ATTACH

namespace Jay.SourceGen.EntityGen;

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