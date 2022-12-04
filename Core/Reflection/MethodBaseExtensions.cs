using System.Reflection;

namespace Jay.SourceGen.Reflection;

public static class MethodBaseExtensions
{
    public static Visibility GetVisibility(this MethodBase method)
    {
        Visibility vis = default;
        if (method.IsPrivate)
            vis |= Visibility.Private;
        if (method.IsFamily)
            vis |= Visibility.Protected;
        if (method.IsAssembly)
            vis |= Visibility.Internal;
        if (method.IsPublic)
            vis |= Visibility.Public;
        return vis;
    }

    public static Access GetAccess(this MethodBase method)
    {
        if (method.IsStatic)
            return Access.Static;
        return Access.Instance;
    }
}