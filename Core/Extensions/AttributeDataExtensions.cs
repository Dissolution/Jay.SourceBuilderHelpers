using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace Jay.SourceGen.Extensions;

public static class AttributeDataExtensions
{
    public static IReadOnlyDictionary<string, object?> GetArgs(this AttributeData attributeData)
    {
        var args = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        var ctorArgs = attributeData.ConstructorArguments;
        if (ctorArgs.Length > 0)
        {
            var ctorParams = attributeData.AttributeConstructor?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty;
            Debug.Assert(ctorArgs.Length == ctorParams.Length);
            int count = ctorArgs.Length;
            for (var i = 0; i < count; i++)
            {
                string name = ctorParams[i].Name;
                Debug.Assert(args.ContainsKey(name) == false);
                object? value;
                var arg = ctorArgs[i];
                if (arg.Kind == TypedConstantKind.Array)
                    value = arg.Values;
                else
                    value = arg.Value;
                args[name] = value;
            }
        }

        var namedArgs = attributeData.NamedArguments;
        if (namedArgs.Length > 0)
        {
            int count = namedArgs.Length;
            for (var i = 0; i < count; i++)
            {
                var arg = namedArgs[i];
                string name = arg.Key;
                Debug.Assert(args.ContainsKey(name) == false);
                object? value;
                if (arg.Value.Kind == TypedConstantKind.Array)
                    value = arg.Value.Values;
                else
                    value = arg.Value.Value;
                args[name] = value;
            }
        }

        return args;
    }

}
