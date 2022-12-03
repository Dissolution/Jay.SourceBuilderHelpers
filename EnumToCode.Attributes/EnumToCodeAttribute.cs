using System;

namespace Jay.SourceGen.Enums;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumToCodeAttribute : Attribute
{
    public string? Code { get; set; } = null;
    public Naming Naming { get; set; } = Naming.Default;
        
    public EnumToCodeAttribute()
    {

    }
}