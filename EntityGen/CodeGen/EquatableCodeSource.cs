﻿using Jay.EntityGen.Attributes;
using Jay.SourceGen.Code;
using Jay.SourceGen.Extensions;

namespace Jay.EntityGen.CodeGen;

internal static partial class CodeSources
{
    public static bool GenerateEquality(EntityInfo entityInfo, out CodeSource? codeSource)
    {
        var equalityMembers = entityInfo.MembersWithAttribute("KeyAttribute");
        if (equalityMembers.Count == 0)
        {
            codeSource = null;
            return false;
        }
        
        using var writer = new CodeWriter();

        string entityType = entityInfo.TypeName;
        string varName = entityInfo.VarName;

        string sealedOrVirt = entityInfo.IsSealed ? "" : " virtual ";

        writer.AutoGeneratedHeader()
            .Nullable(true)
            .NewLine()
            .Using("System.Collections.Generic")
            .NewLine()
            .Namespace(entityInfo.NameSpace)
            .NewLine()
            .CodeBlock($$"""
                partial class {{entityType}} : IEquatable<{{entityType}}>
                {
                    public static bool operator ==({{entityType}}? left, {{entityType}}? right)
                    {
                        if (ReferenceEquals(left, right)) return true;
                        if (left is null || right is null) return false;
                        return left.Equals(right);
                    }                            

                    public static bool operator !=({{entityType}}? left, {{entityType}}? right)
                    {
                        if (ReferenceEquals(left, right)) return false;
                        if (left is null || right is null) return true;
                        return !left.Equals(right);
                    }


                    public{{sealedOrVirt}}bool Equals({{entityType}}? {{varName}})
                    {
                        if ({{varName}} is null) return false;
                        {{(CWA)(w => w.DelimitLines(equalityMembers,
                            (cw, km) => cw.Write(
                            $"if (!EqualityComparer<{km.Type}>.Default.Equals(this.{km.Name}, {varName}.{km.Name})) return false;"))
                        )}}
                        return true;
                    }


                    public override bool Equals(object? obj)
                    {
                        return obj is {{entityType}} {{varName}} && Equals({{varName}});
                    }

                    public override int GetHashCode()
                    {
                        unchecked
                        {
                            int hash = 1009;
                            {{(CWA)(w => w.DelimitLines(equalityMembers, (cw, km) =>
                            {
                                cw.Write("hash = (hash * 9176) + ");
                                if (km.Type.CanBeNull())
                                    cw.Write($"({km.Name}?.GetHashCode() ?? 0);");
                                else
                                    cw.Write($"{km.Name}.GetHashCode();");
                            }))}}
                            return hash;
                        }
                    }
                }
                """);

        string code = writer.ToString();
        codeSource = new($"{entityInfo.Type.GetFQN()}.Equatable.g.cs", code);
        return true;
    }

}