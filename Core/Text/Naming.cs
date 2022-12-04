// ReSharper disable CommentTypo

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
}

public static class NamingExtensions
{
    private static readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

    [return: NotNullIfNotNull(nameof(text))]
    public static string? WithNaming(this string? text, Naming naming)
    {
        if (text is null) return null;
        switch (naming)
        {
            case Naming.Lower:
                return _textInfo.ToLower(text);
            case Naming.Upper:
                return _textInfo.ToUpper(text);
            case Naming.Camel:
            {
                int len = text.Length;
                if (len == 0) return "";
                if (char.IsLower(text[0])) return text;
                Span<char> name = stackalloc char[len];
                name[0] = _textInfo.ToLower(text[0]);
                for (var i = 1; i < len; i++)
                {
                    name[i] = text[i];
                }

                return name.ToString();
            }
            case Naming.Pascal:
            {
                int len = text.Length;
                if (len == 0) return "";
                if (char.IsUpper(text[0])) return text;
                Span<char> name = stackalloc char[len];
                name[0] = _textInfo.ToUpper(text[0]);
                for (var i = 1; i < len; i++)
                {
                    name[i] = text[i];
                }

                return name.ToString();
            }
            case Naming.Title:
                return _textInfo.ToTitleCase(text);
            case Naming.Snake:
            {
                int len = text.Length;
                if (len < 2) return text;
                Span<char> name = stackalloc char[len * 2]; // Aggresive
                // First char
                name[0] = text[0];
                int n = 1;
                // The rest
                for (var i = 1; i < len; i++)
                {
                    char ch = text[i];
                    if (char.IsUpper(ch))
                    {
                        name[n++] = '_';
                    }
                    name[n++] = ch;
                }
                return name[..n].ToString();
            }
            case Naming.Field:
            {
                int len = text.Length;
                if (len == 0) return "";
                Span<char> name = stackalloc char[len + 1];
                name[0] = '_';
                name[1] = _textInfo.ToLower(text[0]);
                int i = 1;
                while (i < len)
                {
                    char ch = text[i];
                    i += 1;
                    name[i] = ch;
                }

                return name.ToString();
            }
            case Naming.Default:
            default:
                return text;
        }
    }
}