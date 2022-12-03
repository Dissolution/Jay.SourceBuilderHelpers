using System.Diagnostics;
using System.Reflection;
using Jay.SourceGen.Enums;

namespace Jay.SourceGen.Text;



[Flags]
public enum CodeFormat
{
    Reference = 0,
    TypeDetails = 1 << 0,
}


public enum MemberFormat
{
    Reference = 0,
    Declaration = 1,
}

[EnumToCode(Naming = Naming.Lower)]
[Flags]
public enum Visibility
{
    None = 0,
    [EnumToCode(Code = "PRIVATE")]
    Private = 1 << 0,
    Protected = 1 << 1,
    Internal  = 1 << 2,
    Public = 1 << 3,
}

[Flags]
public enum Access
{
    None = 0,
    Instance = 1 << 0,
    Static = 1 << 1,
}

// public static partial class ToCodeExtensions
// {
//     public static string ToCode(this Visibility visibility)
//     {
//         return visibility switch
//         {
//             Visibility.Private => "private",
//             Visibility.Protected => "protected",
//             Visibility.Internal => "internal",
//             Visibility.Public => "public",
//             _ => visibility.ToString()
//         };
//     }
// }



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
        //return vis;

        var code = vis.ToEnumCode();

        Debugger.Break();
        return vis;
    }
}

public static class MemberInfoExtensions
{
    public static Visibility GetVisibility(this MemberInfo member)
    {
        throw new NotImplementedException();
    }
}

public static class CodeFormatter
{
    /* Notes:
     * Try to avoid using typeof(T), as it won't work with `object`s
     */

    public static CodeWriter WriteNull<T>(this CodeWriter writer, CodeFormat codeFormat = CodeFormat.Reference)
    {
        if (codeFormat == CodeFormat.TypeDetails)
            writer.Write('(').WriteType(typeof(T)).Write(')');
        return writer.Write("null");
    }

    public static CodeWriter WriteField(this CodeWriter writer, 
        FieldInfo field,
        MemberFormat format = default)
    {
        if (format == MemberFormat.Declaration)
        {

        }
        else
        {

            return writer.WriteType(field.FieldType)
                .Write(' ')
                .WriteType(field.ReflectedType ?? field.DeclaringType)
                .Write('.')
                .Write(field.Name);
        }

        throw new NotImplementedException();
    }

    public static CodeWriter WriteType(this CodeWriter writer, Type? type, CodeFormat codeFormat = CodeFormat.Reference)
    {
        if (type == null)
            return writer.Write("(Type?)null");
        if (type == typeof(bool))
            return writer.Write("bool");
        if (type == typeof(byte))
            return writer.Write("byte");
        if (type == typeof(sbyte))
            return writer.Write("sbyte");
        if (type == typeof(short))
            return writer.Write("short");
        if (type == typeof(ushort))
            return writer.Write("ushort");
        if (type == typeof(int))
            return writer.Write("int");
        if (type == typeof(uint))
            return writer.Write("uint");
        if (type == typeof(long))
            return writer.Write("long");
        if (type == typeof(ulong))
            return writer.Write("ulong");
        if (type == typeof(float))
            return writer.Write("float");
        if (type == typeof(double))
            return writer.Write("double");
        if (type == typeof(decimal))
            return writer.Write("decimal");
        if (type == typeof(char))
            return writer.Write("char");
        if (type == typeof(string))
            return writer.Write("string");
        if (type == typeof(void))
            return writer.Write("void");
        if (type == typeof(object))
            return writer.Write("object");

        // reusable
        Type? underType;

        // Enum: "{nameof(TEnum)}"
        if (type.IsEnum)
        {
            return writer.Write(type.Name);
        }
        
        // Pointer:  "{typeof(T)}*"
        if (type.IsPointer)
        {
            underType = type.GetElementType()!;
            return writer.WriteType(underType).Write('*');
        }

        // ByRef:  "ref {typeof(T)}"
        if (type.IsByRef)
        {
            underType = type.GetElementType()!;
            return writer.Write("ref ").WriteType(underType);
        }

        // Nullable<T>:  "{typeof(T)}?"
        underType = Nullable.GetUnderlyingType(type);
        if (underType is not null)
        {
            return writer.WriteType(underType).Write('?');
        }

        // Array:  "{typeof(T)}[]"
        if (type.IsArray)
        {
            underType = type.GetElementType()!;
            return writer.WriteType(underType).Write("[]");
        }

        // Nested Type?
        if (type.IsNested && !type.IsGenericParameter)
        {
            writer.WriteType(type.DeclaringType!).Write('.');
        }

        // If non-generic
        if (!type.IsGenericType)
        {
            // Just write the type name and we're done
            return writer.Write(type.Name);
        }

        // Start processing type name
        ReadOnlySpan<char> typeName = type.Name.AsSpan();

        // I'm a parameter?
        if (type.IsGenericParameter)
        {
            var constraints = type.GetGenericParameterConstraints();
            if (constraints.Length > 0)
            {
                writer.Write(" : ");
                Debugger.Break();
            }

            Debugger.Break();
        }

        // Name is often something like NAME`2 for NAME<,>, so we want to strip that off
        var i = typeName.IndexOf('`');
        if (i >= 0)
            writer.Write(typeName[..i]);
        else
            writer.Write(typeName);

        var genericTypes = type.GetGenericArguments();
        return writer.Write('<')
                .Delimited(",", genericTypes, static (writer, genericType) => writer.WriteType(genericType))
                .Write('>');
    }


    public static CodeWriter WriteCode<T>(
        this CodeWriter writer,
        T? value,
        CodeFormat codeFormat = CodeFormat.Reference)
    {
        switch (value)
        {
            case null:
                return WriteNull<T>(writer, codeFormat);
            case bool boolean:
                return writer.Write(boolean ? "true" : "false");
            case byte or sbyte or short or ushort:
                return writer.Write('(').WriteType(value.GetType(), codeFormat).Write(')').Write<T>(value);
            case int int32:
                return writer.Write<int>(int32);
            case uint uint32:
                return writer.Write<uint>(uint32).Write('U');
            case long int64:
                return writer.Write<long>(int64).Write('L');
            case ulong uint64:
                return writer.Write<ulong>(uint64).Write("UL");
            case float f:
                return writer.Write<float>(f).Write('f');
            case double d:
                return writer.Write<double>(d).Write('d');
            case decimal m:
                return writer.Write<decimal>(m).Write('m');
            case TimeSpan timeSpan:
                return writer.Write('"').Format<TimeSpan>(timeSpan, "c").Write('"');
            case DateTime dateTime:
                return writer.Write('"').Format<DateTime>(dateTime, "O").Write('"');
            case DateTimeOffset dateTimeOffset:
                return writer.Write('"').Format<DateTimeOffset>(dateTimeOffset, "O").Write('"');
            case Guid guid:
                return writer.Write('"').Format<Guid>(guid, "D").Write('"');
            case char ch:
                return writer.Write('\'').Write(ch).Write('\'');
            case string str:
                return writer.Write('"').Write(str).Write('"');
            case Type type:
                return WriteType(writer, type, codeFormat);
            default:
                break;
        }

        var valueType = value.GetType();
        Debugger.Break();

        if (codeFormat == CodeFormat.TypeDetails)
            writer.Write('(').WriteType(valueType).Write(')');
        return writer.Write<T>(value);
    }


    public static string ToCode<T>(this T? value, CodeFormat codeFormat = CodeFormat.Reference)
    {
        using var writer = new CodeWriter();
        writer.WriteCode<T>(value, codeFormat);
        return writer.ToString();
    }
}

/*
public static class CodeFormatter2
{


    internal static void WriteCodeTo<T>(this T? value, CharArrayWriter writer)
    {
        if (value is null)
        {
            writer.Write('(');
            WriteTypeTo(typeof(T), writer);
            writer.Write(")null");
            return;
        }

        switch (Type.GetTypeCode(typeof(T)))
        {
            case TypeCode.Empty:
                writer.Write("null");
                return;
            case TypeCode.DBNull:
                writer.Write("DBNull.Value");
                return;
            case TypeCode.Boolean:
                writer.Write("(bool)");
                writer.Write<T>(value);
                return;
            case TypeCode.Byte:
                writer.Write("(byte)");
                writer.Write<T>(value);
                return;
            case TypeCode.SByte:
                writer.Write("(sbyte)");
                writer.Write<T>(value);
                return;
            case TypeCode.Int16:
                writer.Write("(short)");
                writer.Write<T>(value);
                return;
            case TypeCode.UInt16:
                writer.Write("(ushort)");
                writer.Write<T>(value);
                return;
            case TypeCode.Int32:
                writer.Write<T>(value);
                return;
            case TypeCode.UInt32:
                writer.Write<T>(value);
                writer.Write('U');
                return;
            case TypeCode.Int64:
                writer.Write<T>(value);
                writer.Write('L');
                return;
            case TypeCode.UInt64:
                writer.Write<T>(value);
                writer.Write("UL");
                return;
            case TypeCode.Single:
                writer.Write<T>(value);
                writer.Write('f');
                return;
            case TypeCode.Double:
                writer.Write<T>(value);
                writer.Write('d');
                return;
            case TypeCode.Decimal:
                writer.Write<T>(value);
                writer.Write('m');
                return;
            case TypeCode.Char:
                writer.Write('\'');
                writer.Write<T>(value);
                writer.Write('\'');
                return;
            case TypeCode.String:
                writer.Write('"');
                writer.Write<T>(value);
                writer.Write('"');
                return;
            case TypeCode.DateTime:
            case TypeCode.Object:
            default:
                break;
        }

        if (value is Type type)
        {
            WriteTypeTo(type, writer);
            return;
        }

        // Default to just the value
        writer.Write<T>(value);
    }

    public static string ToCode<T>(this T? value)
    {
        using var writer = new CharArrayWriter();
        WriteCodeTo<T>(value, writer);
        return writer.ToString();
    }
}*/