using System;

namespace Jay.EntityGen.Attributes;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class EntityAttribute : Attribute
{
    public EntityAttribute()
    {

    }
}