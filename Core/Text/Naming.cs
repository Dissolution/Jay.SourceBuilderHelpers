// ReSharper disable CommentTypo

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp;

namespace Jay.SourceGen.Text;

public enum Naming
{
    /// <summary>
    /// Default naming convention
    /// </summary>
    /// <remarks>
    /// "membername" -> "membername"<br/>
    /// "memberName" -> "memberName"<br/>
    /// "MemberName" -> "MemberName"<br/>
    /// "MEMBERNAME" -> "MEMBERNAME"<br/>
    /// </remarks>
    Default = 0,

    /// <summary>
    /// Lowercase naming
    /// </summary>
    /// <remarks>
    /// "membername" -> "membername"<br/>
    /// "memberName" -> "membername"<br/>
    /// "MemberName" -> "membername"<br/>
    /// "MEMBERNAME" -> "membername"<br/>
    /// </remarks>
    Lower,

    /// <summary>
    /// Uppercase naming
    /// </summary>
    /// <remarks>
    /// "membername" -> "MEMBERNAME"<br/>
    /// "memberName" -> "MEMBERNAME"<br/>
    /// "MemberName" -> "MEMBERNAME"<br/>
    /// "MEMBERNAME" -> "MEMBERNAME"<br/>
    /// </remarks>
    Upper,

    /// <summary>
    /// Camel-cased naming
    /// </summary>
    /// <remarks>
    /// "membername" -> "membername"<br/>
    /// "memberName" -> "memberName"<br/>
    /// "MemberName" -> "memberName"<br/>
    /// "MEMBERNAME" -> "mEMBERNAME"<br/>
    /// </remarks>
    Camel,

    /// <summary>
    /// Pascal-cased naming
    /// </summary>
    /// <remarks>
    /// "membername" -> "Membername"<br/>
    /// "memberName" -> "MemberName"<br/>
    /// "MemberName" -> "MemberName"<br/>
    /// "MEMBERNAME" -> "MEMBERNAME"<br/>
    /// </remarks>
    Pascal,

    Title,

    /// <summary>
    /// Snake case naming
    /// </summary>
    /// <remarks>
    /// "membername" -> "membername"<br/>
    /// "memberName" -> "member_Name"<br/>
    /// "MemberName" -> "Member_Name"<br/>
    /// "MEMBERNAME" -> "MEMBERNAME"<br/>
    /// </remarks>
    Snake,

    /// <summary>
    /// C# private field naming convention
    /// </summary>
    /// <remarks>
    /// "membername" -> "_membername"<br/>
    /// "memberName" -> "_memberName"<br/>
    /// "MemberName" -> "_memberName"<br/>
    /// "MEMBERNAME" -> "_mEMBERNAME"<br/>
    /// </remarks>
    Field,

    Variable,

}

public static class NamingExtensions
{
    internal static readonly TextInfo TextInfo = CultureInfo.CurrentCulture.TextInfo;

    [return: NotNullIfNotNull(nameof(text))]
    public static string? WithNaming(this string? text, Naming naming)
    {
        if (text is null) return null;
        switch (naming)
        {
            case Naming.Lower:
                return TextInfo.ToLower(text);
            case Naming.Upper:
                return TextInfo.ToUpper(text);
            case Naming.Camel or Naming.Variable:
            {
                int len = text.Length;
                if (len == 0) return "";
                string name;
                if (!char.IsLower(text[0]))
                {
                    Span<char> nameBuffer = stackalloc char[len];
                    nameBuffer[0] = TextInfo.ToLower(text[0]);
                    text.AsSpan(1).CopyTo(nameBuffer.Slice(1));
                    name = nameBuffer.ToString();
                }
                else
                {
                    name = text;
                }

                if (naming == Naming.Variable)
                {
                    if (!SyntaxFacts.IsValidIdentifier(name))
                        return $"@{name}";
                }

                return name;
            }
            case Naming.Pascal:
            {
                int len = text.Length;
                if (len == 0) return "";
                if (char.IsUpper(text[0])) return text;
                Span<char> nameBuffer = stackalloc char[len];
                nameBuffer[0] = TextInfo.ToUpper(text[0]);
                text.AsSpan(1).CopyTo(nameBuffer.Slice(1));
                return nameBuffer.ToString();
            }
            case Naming.Title:
                return TextInfo.ToTitleCase(text);
            case Naming.Snake:
            {
                int len = text.Length;
                if (len < 2) return text;
                Span<char> nameBuffer = stackalloc char[len * 2]; // Aggressive
                // First char
                nameBuffer[0] = text[0];
                int n = 1;
                // The rest
                for (var i = 1; i < len; i++)
                {
                    char ch = text[i];
                    if (char.IsUpper(ch))
                    {
                        nameBuffer[n++] = '_';
                    }
                    nameBuffer[n++] = ch;
                }
                return nameBuffer.Slice(0, n).ToString();
            }
            case Naming.Field:
            {
                int len = text.Length;
                if (len == 0) return "";
                Span<char> nameBuffer = stackalloc char[len + 1];
                nameBuffer[0] = '_';
                nameBuffer[1] = TextInfo.ToLower(text[0]);
                text.AsSpan(1).CopyTo(nameBuffer.Slice(2));
                return nameBuffer.ToString();
            }
            case Naming.Default:
            default:
                return text;
        }
    }
}