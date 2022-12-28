/*using System.Diagnostics;
using System.Reflection;
using Jay.SourceGen.Reflection;

namespace Jay.SourceGen.Code;

public static class CodeWriterExtensions
{
    public static CodeWriter WriteNull<T>(this CodeWriter writer, CodeFormat codeFormat = default)
    {
        if (codeFormat == CodeFormat.Declaration)
            writer.Write('(').WriteType(typeof(T)).Write(')');
        return writer.Write("null");
    }
}


public static class CodeFormatter
{
    /* Notes:
     * Try to avoid using typeof(T), as it won't work with `object`s
     #1#

    private static CodeWriter WriteVisibility(this CodeWriter codeWriter, Visibility visibility)
    {
        if (visibility.HasFlag(Visibility.Private))
        {
            codeWriter.Write("private ");
        }

        if (visibility.HasFlag(Visibility.Protected))
        {
            codeWriter.Write("protected ");
        }

        if (visibility.HasFlag(Visibility.Internal))
        {
            codeWriter.Write("internal ");
        }

        if (visibility.HasFlag(Visibility.Public))
        {
            codeWriter.Write("public ");
        }

        return codeWriter;
    }

    public static CodeWriter WriteAccess(this CodeWriter writer, Access access)
    {
        if (access == Access.Static)
        {
            return writer.Write("static ");
        }
        else
        {
            return writer;
        }
    }

    public static CodeWriter WriteField(this CodeWriter writer,
        FieldInfo field,
        MemberFormat format = default)
    {
        if (format == MemberFormat.Declaration)
        {
            writer.WriteVisibility(field.GetVisibility())
                .WriteAccess(field.GetAccess());
        }

        return writer.WriteType(field.FieldType)
            .Write(' ')
            .WriteType(field.OwningType())
            .Write('.')
            .Write(field.Name);
    }

    public static CodeWriter WriteProperty(this CodeWriter writer,
        PropertyInfo property,
        MemberFormat format = default)
    {
        if (format != MemberFormat.Declaration)
        {
            return writer.WriteType(property.PropertyType)
                .Write(' ')
                .WriteType(property.OwningType())
                .Write('.')
                .Write(property.Name);
        }

        var access = property.GetAccess();
        var visibility = property.GetVisibility();
        writer.WriteVisibility(visibility)
            .WriteAccess(access)
            .WriteType(property.PropertyType)
            .Write(' ')
            .WriteType(property.OwningType())
            .Write('.')
            .Write(property.Name)
            .Write(" { ");
        var getter = property.Getter();
        if (getter is not null)
        {
            var getVis = getter.GetVisibility();
            if (getVis != visibility)
                writer.WriteVisibility(getVis);
            writer.Write("get; ");
        }

        var setter = property.Setter();
        if (setter is not null)
        {
            var setVis = setter.GetVisibility();
            if (setVis != visibility)
                writer.WriteVisibility(setVis);
            writer.Write("set; ");
        }

        return writer.Write('}');
    }

    public static CodeWriter WriteEvent(this CodeWriter writer,
        EventInfo eventInfo,
        MemberFormat format = default)
    {
        if (format == MemberFormat.Declaration)
        {
            writer.WriteVisibility(eventInfo.GetVisibility())
                .WriteAccess(eventInfo.GetAccess());
        }

        return writer.WriteType(eventInfo.EventHandlerType)
            .Write(' ')
            .WriteType(eventInfo.OwningType())
            .Write('.')
            .Write(eventInfo.Name);
    }


    public static CodeWriter WriteConstructor(this CodeWriter writer,
        ConstructorInfo constructor,
        MemberFormat format = default)
    {
        if (format == MemberFormat.Declaration)
        {
            var access = constructor.GetAccess();
            if (access == Access.Static)
            {
                writer.WriteAccess(access);
            }
            else
            {
                var visibility = constructor.GetVisibility();
                writer.WriteVisibility(visibility);
            }
        }

        return writer.WriteType(constructor.DeclaringType!)
            .Write('(')
            .Delimited(", ", constructor.GetParameters(), static (w, p) => w.WriteParameter(p))
            .Write(')');
    }

    public static CodeWriter WriteMethod(this CodeWriter writer,
        MethodInfo method,
        MemberFormat format = default)
    {
        if (format == MemberFormat.Declaration)
        {
            writer.WriteVisibility(method.GetVisibility())
                .WriteAccess(method.GetAccess());
        }

        return writer.WriteType(method.ReturnType)
            .Write(' ')
            .WriteType(method.OwningType())
            .Write('.')
            .Write(method.Name)
            .Write('(')
            .Delimited(", ", method.GetParameters(), static (w, p) => w.WriteParameter(p))
            .Write(')');
    }

    public static CodeWriter WriteType(this CodeWriter writer, Type? type)
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

    public static CodeWriter WriteParameter(this CodeWriter writer,
        ParameterInfo parameter)
    {
        Type parameterType = parameter.ParameterType;
        if (parameterType.IsByRef)
        {
            if (parameter.IsIn)
            {
                writer.Write("in ");
            }
            else if (parameter.IsOut)
            {
                writer.Write("out ");
            }
            else
            {
                writer.Write("ref ");
            }

            parameterType = parameterType.GetElementType()!;
        }

        writer.WriteType(parameterType)
            .Write(' ')
            .Write(parameter.Name ?? "???");

        if (parameter.HasDefaultValue)
        {
            writer.Write(" = ")
                .Write(parameter.DefaultValue);
        }

        return writer;
    }

    public static CodeWriter WriteArray(this CodeWriter writer, Array? array, CodeFormat format = default)
    {
        if (array is null)
        {
            return writer.WriteNull<Array>(format);
        }
        var elementType = array.GetType().GetElementType()!;
        return writer.WriteType(elementType)
            .Write('[')
            .Delimited(",", array.Cast<object?>())
            .Write(']');
    }


    public static CodeWriter WriteCode<T>(
        this CodeWriter writer,
        T? value,
        CodeFormat codeFormat = CodeFormat.Reference)
    {
        switch (value)
        {
            case null:
                return writer.WriteNull<T>(codeFormat);
            case bool boolean:
                return writer.Write(boolean ? "true" : "false");
            case byte or sbyte or short or ushort:
                return writer.Write('(').WriteType(value.GetType()).Write(')').Write(value);
            case int int32:
                return writer.Write(int32);
            case uint uint32:
                return writer.Write(uint32).Write('U');
            case long int64:
                return writer.Write(int64).Write('L');
            case ulong uint64:
                return writer.Write(uint64).Write("UL");
            case float f:
                return writer.Write(f).Write('f');
            case double d:
                return writer.Write(d).Write('d');
            case decimal m:
                return writer.Write(m).Write('m');
            case TimeSpan timeSpan:
                return writer.Write('"').Format(timeSpan, "c").Write('"');
            case DateTime dateTime:
                return writer.Write('"').Format(dateTime, "O").Write('"');
            case DateTimeOffset dateTimeOffset:
                return writer.Write('"').Format(dateTimeOffset, "O").Write('"');
            case Guid guid:
                return writer.Write('"').Format(guid, "D").Write('"');
            case char ch:
                return writer.Write('\'').Write(ch).Write('\'');
            case string str:
                return writer.Write('"').Write(str).Write('"');
            case Type type:
                return writer.WriteType(type);
            case FieldInfo field:
                return writer.WriteField(field);
            case PropertyInfo property:
                return writer.WriteProperty(property);
            case EventInfo eventInfo:
                return writer.WriteEvent(eventInfo);
            case ConstructorInfo constructor:
                return writer.WriteConstructor(constructor);
            case MethodInfo method:
                return writer.WriteMethod(method);
            case ParameterInfo parameter:
                return writer.WriteParameter(parameter);
            case CWA cwa:
            {
                var curIndent = writer.CurrentLineIndent();
                return writer.IndentBlock(curIndent, cwa);
                //cwa(writer);
                //return writer;
            }
            case IEnumerable enumerable:
            {
                return writer.AppendDelimit(", ", enumerable.Cast<object?>());
            }
            default:
                break;
        }

        var valueType = value.GetType();
        if (valueType.IsEnum)
        {
            // Todo: make this faster!
            string name = Enum.GetName(valueType, value) ?? "TEnum";
            return writer.Write(name);
        }

        if (valueType.IsArray)
        {
            return writer.WriteArray(value as Array);
        }

        
        // Complex member?
        if (codeFormat == CodeFormat.Declaration)
        {
            Debugger.Break();
        }


        if (codeFormat == CodeFormat.Declaration)
            writer.Write('(').WriteType(valueType).Write(')');
        return writer.Write(value);
    }


    public static string ToCode<T>(this T? value, CodeFormat codeFormat = CodeFormat.Reference)
    {
        using var writer = new CodeWriter();
        writer.WriteCode(value, codeFormat);
        return writer.ToString();
    }
}*/