using System.Reflection;

namespace Jay.SourceGen.Reflection;

public static class FieldInfoExtensions
{
    public static Visibility GetVisibility(this FieldInfo field)
    {
        Visibility vis = default;
        if (field.IsPrivate)
            vis |= Visibility.Private;
        if (field.IsFamily)
            vis |= Visibility.Protected;
        if (field.IsAssembly)
            vis |= Visibility.Internal;
        if (field.IsPublic)
            vis |= Visibility.Public;
        return vis;
    }

    public static Access GetAccess(this FieldInfo field)
    {
        if (field.IsStatic)
            return Access.Static;
        return Access.Instance;
    }
}