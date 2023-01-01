using System;

namespace Jay.EntityGen.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class KeyAttribute : Attribute
{
    public KeyKind Kind { get; set; }

    public KeyAttribute(KeyKind kind = KeyKind.Id)
    {
        this.Kind = kind;
    }
}       