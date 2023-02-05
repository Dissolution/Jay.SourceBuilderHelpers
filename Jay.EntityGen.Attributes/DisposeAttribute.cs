using System;

namespace Jay.EntityGen.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
public sealed class DisposeAttribute : Attribute
{
    public DisposeAttribute()
    {

    }
}