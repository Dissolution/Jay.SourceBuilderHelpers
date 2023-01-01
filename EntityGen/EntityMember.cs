using Jay.EntityGen.Attributes;

namespace Jay.EntityGen;

internal sealed record class EntityMember(string Name, ITypeSymbol Type, KeyKind Kind);
