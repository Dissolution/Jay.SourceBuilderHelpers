using System;

namespace Jay.EntityGen.Attributes;

[Flags]
public enum KeyKind
{
    None = 0,
    Equality   = 1 << 0,
    Display    = 1 << 1,
    Id = Equality | Display,
    Dispose = 1 << 2,
}
