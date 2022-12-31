using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Jay.SourceGen;

public static class Hasher
{
    public static int GetHashCode<T>(IReadOnlyList<T> values)
    {
        unchecked
        {
            int hash = 1009;
            int count = values.Count;
            for (var i = 0; i < count; i++)
            {
                var itemHash = (values[i]?.GetHashCode() ?? 0);
                hash = (hash * 9176) + itemHash;
            }
            return hash;
        }
    }

    public static int GetHashCode<T>(IEnumerable<T> values)
    {
        unchecked
        {
            int hash = 1009;
            foreach (var value in values)
            {
                var itemHash = (value?.GetHashCode() ?? 0);
                hash = (hash * 9176) + itemHash;
            }          
            return hash;
        }
    }
}

