using Jay.EntityGen.Attributes;
using Jay.SourceGen.Text;

namespace Jay.EntityGen;

internal sealed record class EntityMemberInfo(string Name, ITypeSymbol Type, KeyKind Kind)
{
    public string VarName => Name.WithNaming(Naming.Variable);
}
