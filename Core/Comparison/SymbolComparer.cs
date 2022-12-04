using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Jay.SourceGen.Comparison;

public static class SymbolComparer
{
    public static bool Equals(ISymbol? left, ISymbol? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right, SymbolEqualityComparer.Default);
    }
}