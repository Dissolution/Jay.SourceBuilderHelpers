using System;

namespace Jay.EntityGen.Attributes;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class EntityAttribute : Attribute
{
    public bool Nullability { get; set; } = true;

    public EntityAttribute()
    {
        
    }
}
