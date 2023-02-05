using Jay.EntityGen.Attributes;
using Jay.SourceGen.Text;

namespace Jay.EntityGen;

internal sealed record class EntityMemberInfo(string Name, ITypeSymbol Type, IReadOnlyDictionary<string, object?> AttributeData)
{
    public string VarName => Name.WithNaming(Naming.Variable);
}
