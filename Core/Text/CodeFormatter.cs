namespace Jay.SourceBuilderHelpers.Text;

public static class CodeFormatter
{
    internal static void WriteCodeTo(this Type type, CharArrayWriter writer)
    {
        // Write C# type definition
        Type? underType;

        // Nullable check
        underType = Nullable.GetUnderlyingType(type);
        if (underType is not null)
        {
            WriteCodeTo(underType, writer);
            writer.Write('?');
            return;
        }

        if (type.IsArray)
        {
            underType = type.GetElementType()!;
            WriteCodeTo(underType, writer);
            writer.Write("[]");
            return;
        }

        if (type.IsGenericType)
        {
            var genericTypes = type.GetGenericArguments();
            var i = type.Name.IndexOf('`');
            if (i >= 0)
            {
                writer.Write(type.Name.Substring(0, i));
            }
            else
            {
                writer.Write(type.Name);
            }

            writer.Write('<');
            writer.Delimited(",", genericTypes, static (writer, t) => WriteCodeTo(t, writer));
            writer.Write('>');
            return;
        }

        if (type == typeof(bool))
        {
            writer.Write("bool");
            return;
        }

        if (type == typeof(byte))
        {
            writer.Write("byte");
            return;
        }

        if (type == typeof(sbyte))
        {
            writer.Write("sbyte");
            return;
        }

        if (type == typeof(short))
        {
            writer.Write("short");
            return;
        }

        if (type == typeof(ushort))
        {
            writer.Write("ushort");
            return;
        }

        if (type == typeof(int))
        {
            writer.Write("int");
            return;
        }

        if (type == typeof(uint))
        {
            writer.Write("uint");
            return;
        }

        if (type == typeof(long))
        {
            writer.Write("long");
            return;
        }

        if (type == typeof(ulong))
        {
            writer.Write("ulong");
            return;
        }

        if (type == typeof(float))
        {
            writer.Write("float");
            return;
        }

        if (type == typeof(double))
        {
            writer.Write("double");
            return;
        }

        if (type == typeof(decimal))
        {
            writer.Write("decimal");
            return;
        }

        if (type == typeof(char))
        {
            writer.Write("char");
            return;
        }

        if (type == typeof(string))
        {
            writer.Write("string");
            return;
        }

        if (type == typeof(object))
        {
            writer.Write("object");
            return;
        }

        if (type == typeof(void))
        {
            writer.Write("void");
            return;
        }

        writer.Write(type.Name);
    }

    internal static void WriteCodeTo<T>(this T? value, CharArrayWriter writer)
    {
        if (value is null)
        {
            writer.Write('(');
            WriteCodeTo(typeof(T), writer);
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
            WriteCodeTo(type, writer);
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
}