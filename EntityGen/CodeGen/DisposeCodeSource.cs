﻿using Jay.EntityGen.Attributes;
using Jay.SourceGen.Code;
using Jay.SourceGen.Extensions;

namespace Jay.EntityGen.CodeGen;

internal static partial class CodeSources
{
    public static bool GenerateDisposable(EntityInfo entityInfo, out CodeSource? codeSource)
    {
        var disposeMembers = entityInfo.MembersWithAttribute<DisposeAttribute>();
        if (disposeMembers.Count == 0)
        {
            codeSource = null;
            return false;
        }

        using var writer = new CodeWriter();

        string entityType = entityInfo.TypeName;

        writer.AutoGeneratedHeader()
            .Nullable(true)
            .NewLine()
            .Namespace(entityInfo.NameSpace)
            .NewLine()
            .CodeBlock($$"""
                partial class {{entityType}}
                {
                    public{{(entityInfo.IsSealed ? " " : " virtual ")}} void Dispose()
                    {
                        {{(CWA)(cw => cw.DelimitLines(disposeMembers, static (w, m) => w.Write($"this.{m.Name} = default!;")))}}
                    }
                }
                """);

        string code = writer.ToString();
        codeSource = new($"{entityInfo.Type.GetFQN()}.Disposal.g.cs", code);
        return true;
    }
}