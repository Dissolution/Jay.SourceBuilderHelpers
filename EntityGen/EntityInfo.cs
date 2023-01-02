using Jay.EntityGen.Attributes;
using Jay.SourceGen;
using Jay.SourceGen.Extensions;

namespace Jay.EntityGen;

internal sealed class EntityInfo
{
    public ITypeSymbol Type { get; }
    public IReadOnlyList<EntityMember> Members { get; }
    public bool Nullability { get; }

    public string? NameSpace => Type.GetNamespace();
    public string Name => Type.Name;
    public string VarName => Type.GetVariableName();

    public EntityInfo(ITypeSymbol type, List<EntityMember> keyMembers, bool nullability)
    {
        Type = type;
        Members = keyMembers;
        Nullability = nullability;
    }

    public IEnumerable<EntityMember> FindMembers(KeyKind kind)
    {
        return Members.Where(member => member.Kind.HasFlag(kind));
    }
}
