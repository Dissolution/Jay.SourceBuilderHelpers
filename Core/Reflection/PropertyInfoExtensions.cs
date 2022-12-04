using System.Reflection;

namespace Jay.SourceGen.Reflection;

public static class PropertyInfoExtensions
{
    public static MethodInfo? Getter(this PropertyInfo property)
    {
        return property.GetGetMethod(false) ??
            property.GetGetMethod(true);
    }

    public static MethodInfo? Setter(this PropertyInfo property)
    {
        return property.GetSetMethod(false) ??
            property.GetSetMethod(true);
    }

    public static Visibility GetVisibility(this PropertyInfo property)
    {
        Visibility vis = default;
        var getMethod = property.Getter();
        if (getMethod is not null)
            vis |= getMethod.GetVisibility();
        var setMethod = property.Setter();
        if (setMethod is not null)
            vis |= setMethod.GetVisibility();
        return vis;
    }

    public static Access GetAccess(this PropertyInfo property)
    {
        Access access = default;
        var getMethod = property.Getter();
        if (getMethod is not null)
            access |= getMethod.GetAccess();
        var setMethod = property.Setter();
        if (setMethod is not null)
            access |= setMethod.GetAccess();
        return access;
    }
}