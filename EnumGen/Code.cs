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
}