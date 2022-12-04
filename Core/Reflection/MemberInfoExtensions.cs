using System.Reflection;

namespace Jay.SourceGen.Reflection;

public static class MemberInfoExtensions
{
    public static Type OwningType(this MemberInfo member)
    {
        Type? owner = member.ReflectedType;
        if (owner is not null) return owner;
        owner = member.DeclaringType;
        if (owner is not null) return owner;
        owner = member.Module.GetType();
        return owner;
    }

    public static Visibility GetVisibility(this MemberInfo member)
    {
        if (member is FieldInfo field) return field.GetVisibility();
        if (member is PropertyInfo property) return property.GetVisibility();
        if (member is EventInfo eventInfo) return eventInfo.GetVisibility();
        if (member is MethodBase method) return method.GetVisibility();
        throw new ArgumentOutOfRangeException(nameof(member));
    }

    public static Access GetAccess(this MemberInfo member)
    {
        if (member is FieldInfo field) return field.GetAccess();
        if (member is PropertyInfo property) return property.GetAccess();
        if (member is EventInfo eventInfo) return eventInfo.GetAccess();
        if (member is MethodBase method) return method.GetAccess();
        throw new ArgumentOutOfRangeException(nameof(member));
    }
}