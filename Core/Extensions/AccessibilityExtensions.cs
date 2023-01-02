using Microsoft.CodeAnalysis;

namespace Jay.SourceGen.Extensions;

public static class AccessibilityExtensions
{
    public static string ToCode(this Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Private:
                return "private";
            case Accessibility.ProtectedOrInternal:
            case Accessibility.ProtectedAndInternal:
                return "protected internal";
            case Accessibility.Protected:
                return "protected";
            case Accessibility.Internal:
                return "internal";
            case Accessibility.Public:
                return "public";
            case Accessibility.NotApplicable:
            default:
                return "";
        }
    }
}
