using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace Jay.SourceGen.Extensions;

public static class AttributeDataExtensions
{
    public static AttributeData? Find(this ImmutableArray<AttributeData> attributes, string attributeName)
    {
        for (int i = 0; i < attributes.Length; i++)
        {
            string? attrName = attributes[i].AttributeClass?.Name;
            if (string.Equals(attrName, attributeName))
                return attributes[i];
        }
        return null;
    }

    internal static object? GetObjectValue(this TypedConstant typedConstant)
    {
        if (typedConstant.Kind == TypedConstantKind.Array)
        {
            var values = typedConstant.Values;
            object?[] array = new object?[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                array[i] = GetObjectValue(values[i]);
            }
            return array;
        }
        else
        {
            return typedConstant.Value;
        }
    }

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
                object? value = ctorArgs[i].GetObjectValue();
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
                object? value = arg.Value.GetObjectValue();
                args[name] = value;
            }
        }

        return args;
    }

}
