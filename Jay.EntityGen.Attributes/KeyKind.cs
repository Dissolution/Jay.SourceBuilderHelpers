using System;

namespace Jay.EntityGen.Attributes;

[Flags]
public enum KeyKind
{
    None = 0,
    Equality   = 1 << 0,
    Comparison = 1 << 1,
    Display    = 1 << 2,

    Id = Equality | Comparison | Display,
}
