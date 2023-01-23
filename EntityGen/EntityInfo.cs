using Jay.EntityGen.Attributes;
using Jay.SourceGen.Extensions;
using Jay.SourceGen.Text;

namespace Jay.EntityGen;

internal sealed class EntityInfo
{
    public required ITypeSymbol Type { get; init; }
    public required IReadOnlyList<EntityMemberInfo> Members { get; init; }
    public required bool IsSealed { get; init; }
    public bool IsNotify { get; init; } = false;

    public string? NameSpace => Type.GetNamespace();
    public string TypeName => Type.Name;
    public string VarName => Type.Name.WithNaming(Naming.Variable);
    
    public IReadOnlyList<EntityMemberInfo> MembersOfKind(KeyKind kind)
    {
        return Members.Where(member => member.Kind.HasFlag(kind)).ToList();
    }
}
