using Jay.EntityGen.Attributes;
using Jay.SourceGen;
using Jay.SourceGen.Code;
using Jay.SourceGen.Extensions;

namespace Jay.EntityGen.CodeGen;

internal static class ComparableCodeSource
{
    
    public static  CodeSource Generate(EntityInfo entityInfo)
    {
        using var writer = new CodeWriter();

        string entityType = entityInfo.Name;
        string varName = entityInfo.VarName;
        var keyMembers = entityInfo.Members;

        var compareMember = keyMembers.Single(m => m.Kind.HasFlag(KeyKind.Comparison));
        var memberName = compareMember.Name;
        var memberType = compareMember.Type;

        writer.AutoGeneratedHeader()
            .Nullable(entityInfo.Nullability)
            .NewLine()
            .Using("System.Collections.Generic")
            .NewLine()
            .Namespace(entityInfo.NameSpace)
            .NewLine()
            .CodeBlock($$"""
                partial class {{entityType}} : IComparable<{{entityType}}>
                {
                    public static bool operator <({{entityType}} left, {{entityType}} right) => 
                        Comparer<{{memberType}}>.Default.Compare(left.{{memberName}}, right.{{memberName}}) < 0;
                
                    public static bool operator <=({{entityType}} left, {{entityType}} right) => 
                        Comparer<{{memberType}}>.Default.Compare(left.{{memberName}}, right.{{memberName}}) <= 0;
                    
                    public static bool operator >({{entityType}} left, {{entityType}} right) => 
                        Comparer<{{memberType}}>.Default.Compare(left.{{memberName}}, right.{{memberName}}) > 0;
                    
                    public static bool operator >=({{entityType}} left, {{entityType}} right) => 
                        Comparer<{{memberType}}>.Default.Compare(left.{{memberName}}, right.{{memberName}}) >= 0;
                    
                    
                    public int CompareTo({{entityType}}? {{varName}})
                    {
                        // Nulls sort first
                        if ({{varName}} == null) return 1;
                        return Comparer<{{memberType}}>.Default.Compare(this.{{memberName}}, {{varName}}.{{memberName}});
                    }
                }
                """);

        string code = writer.ToString();
        return new($"{entityInfo.Type.GetFQN()}.Comparable.g.cs", code);
    }

}
