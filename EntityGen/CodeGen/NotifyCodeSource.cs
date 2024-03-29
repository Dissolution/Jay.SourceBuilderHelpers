﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using Jay.EntityGen.Attributes;
using Jay.SourceGen.Code;
using Jay.SourceGen.Extensions;

namespace Jay.EntityGen.CodeGen;

internal static partial class CodeSources
{
    public static bool GenerateNotification(EntityInfo entityInfo, out CodeSource? codeSource)
    {
        if (!entityInfo.IsNotify)
        {
            codeSource = null;
            return false;
        }

        using var writer = new CodeWriter();

        string entityType = entityInfo.TypeName;
        string varName = entityInfo.VarName;

        writer.AutoGeneratedHeader()
            .Nullable(true)
            .NewLine()
            .Using("System.ComponentModel")
            .Using("System.Runtime.CompilerServices")
            .NewLine()
            .Namespace(entityInfo.NameSpace)
            .NewLine()
            .CodeBlock($$"""
                partial class {{entityType}} : INotifyPropertyChanged, INotifyPropertyChanging
                {
                    public event PropertyChangedEventHandler? PropertyChanged;

                    public event PropertyChangingEventHandler? PropertyChanging;
                }
                """);

        string code = writer.ToString();
        codeSource = new($"{entityInfo.Type.GetFQN()}.Comparable.g.cs", code);
        return true;
    }

}

internal class TestClass : INotifyPropertyChanged, INotifyPropertyChanging
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;

    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    protected void OnPropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

}