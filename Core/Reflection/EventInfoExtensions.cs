using System.Reflection;

namespace Jay.SourceGen.Reflection;

public static class EventInfoExtensions
{
    public static MethodInfo? Adder(this EventInfo eventInfo)
    {
        return eventInfo.GetAddMethod(false) ?? eventInfo.GetAddMethod(true);
    }

    public static MethodInfo? Remover(this EventInfo eventInfo)
    {
        return eventInfo.GetRemoveMethod(false) ?? eventInfo.GetRemoveMethod(true);
    }

    public static MethodInfo? Raiser(this EventInfo eventInfo)
    {
        return eventInfo.GetRaiseMethod(false) ?? eventInfo.GetRaiseMethod(true);
    }

    public static Visibility GetVisibility(this EventInfo eventInfo)
    {
        Visibility vis = default;
        var addMethod = eventInfo.Adder();
        if (addMethod is not null)
            vis |= addMethod.GetVisibility();
        var removeMethod = eventInfo.Remover();
        if (removeMethod is not null)
            vis |= removeMethod.GetVisibility();
        var raiseMethod = eventInfo.Raiser();
        if (raiseMethod is not null)
            vis |= raiseMethod.GetVisibility();
        return vis;
    }

    public static Access GetAccess(this EventInfo eventInfo)
    {
        Access access = default;
        var addMethod = eventInfo.Adder();
        if (addMethod is not null)
            access |= addMethod.GetAccess();
        var removeMethod = eventInfo.Remover();
        if (removeMethod is not null)
            access |= removeMethod.GetAccess();
        var raiseMethod = eventInfo.Raiser();
        if (raiseMethod is not null)
            access |= raiseMethod.GetAccess();
        return access;
    }
}