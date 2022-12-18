﻿namespace Jay.EnumGen;

internal static class Code
{
    public const string EnumExtensions_Code = """
        // <auto-generated/>
        
        public static partial class EnumExtensions
        {
            internal static int PopCount(long value)
            {
                int count = 0;
                while (value != 0L)
                {
                    value &= (value - 1L);
                    count++;
                }
                return count;
            }
        }
        """;

    public const string AttributeName = "ExtendEnumAttribute";
    public const string AttributeCode = """
        // <auto-generated/>
        namespace Jay.EnumGen;
        
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
        public sealed class ExtendEnumAttribute : Attribute
        {
            public ExtendEnumAttribute()
            {

            }
        }
        """;

    public const string EnumGenerationAttributeCode = """
        // <auto-generated/>
        namespace Jay.EnumGen.SmartEnums;

        [AttributeUsage(AttributeTargets.Class)]
        public class EnumGenerationAttribute : Attribute
        {
        }
        """;
}