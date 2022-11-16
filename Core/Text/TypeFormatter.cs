namespace Jay.SourceBuilderHelpers.Text;

public static class TypeFormatter
{
    internal static void WriteCode(CharArrayWriter writer, Type type)
    {
        // Write C# type definition
        Type? underType;

        // Nullable check
        underType = Nullable.GetUnderlyingType(type);
        if (underType is not null)
        {
            WriteCode(writer, underType);
            writer.Write('?');
            return;
        }

        if (type.IsArray)
        {
            underType = type.GetElementType()!;
            WriteCode(writer, underType);
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
            writer.Delimited(",", genericTypes, static (writer, t) => WriteCode(writer, t));
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
        return;
    }

    public static string ToCode(this Type type)
    {
        using var writer = CharArrayWriter.Rent();
        WriteCode(writer, type);
        return writer.ToString();
    }
}