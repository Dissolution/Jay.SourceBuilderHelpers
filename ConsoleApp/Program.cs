﻿using System.Diagnostics;
using System.Numerics;
using Jay.SourceGen.Code;
using Jay.EnumGen;

#if RELEASE
var config = DefaultConfig.Instance
    .AddJob(Job.InProcess
        .WithStrategy(RunStrategy.Throughput)
        .WithRuntime(ClrRuntime.Net461)
        .WithRuntime(CoreRuntime.Core20));

var result = BenchmarkRunner.Run<TextCopyBenchmarks>(config);
var outputPath = result.ResultsDirectoryPath;

//Process.Start(outputPath);
#else
//
//
// using var codeWriter = new CodeWriter();
// /*
// codeWriter
//     .Using("using System;")
//     .WriteLine("namespace Jay.Testing;")
//     .WriteLine()
//     .WriteLine("public class Testing()")
//     .BracketBlock(code =>
//     {
//         code.Parse(@"public string ToString()
// {
//     return ""Testing"";
// }
//
// ")
//             .WriteLine("public void DoThing()")
//             .BracketBlock(bb =>
//             {
//                 bb.Format(@$"
// // Don't do this at home, kids!
// int threadId = {Environment.CurrentManagedThreadId};
// Console.WriteLine($""{{threadId}}"");");
//             }).NewLine();
//     }).NewLine();
//
// */
//
// var method = typeof(Program).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
//     .First();
//
// codeWriter.WriteAutoGeneratedHeader()
//     .Using("System")
//     .WriteLine($"namespace {method.DeclaringType?.Namespace}")
//     .BracketBlock(c => c
//         .WriteLine($"public static partial class {method.DeclaringType?.Name}")
//         .BracketBlock(c => c
//             .WriteLine("static partial void HelloFrom(string name)")
//             .BracketBlock(c => c
//                 .WriteLine("int i = 147;")
//                 .WriteLine("Console.WriteLine($\"Generator says: Hi from '{name}'  #{i}\");"))));
//
//
// IEnumerable<int> enumbable = new List<int> { 1, 2, 3 };
// var str = enumbable.ToString();
//
//
//
// string code = codeWriter.ToString();




using var writer = new CodeWriter();

writer.Append($"This is a complex string with arg holes {147} {new char[] { '1', '2' }}");

Debugger.Break();

namespace ConsoleApp
{
    [ExtendEnum]
    public readonly partial struct TENUM
    {

    }

    public readonly partial struct TENUM :
        IEqualityOperators<TENUM, TENUM, bool>, IEquatable<TENUM>,
        IComparisonOperators<TENUM, TENUM, bool>, IComparable<TENUM>,
        ISpanParsable<TENUM>, IParsable<TENUM>,
        ISpanFormattable, IFormattable
    {
        public static bool operator ==(TENUM left, TENUM right) => left._value == right._value;
        public static bool operator !=(TENUM left, TENUM right) => left._value != right._value;
        public static bool operator >(TENUM left, TENUM right) => left._value > right._value;
        public static bool operator >=(TENUM left, TENUM right) => left._value >= right._value;
        public static bool operator <(TENUM left, TENUM right) => left._value < right._value;
        public static bool operator <=(TENUM left, TENUM right) => left._value <= right._value;

        private static readonly TENUM[] _members;
        private static readonly string[] _membersNames;

        static TENUM()
        {
            //GEN:
            int count = 8;

            _members = new TENUM[0]
            {
                // consts
            };
            _membersNames = new string[1]
            {
                "nameof()",
            };
        }

        public static TENUM[] Members => _members;
        public static string[] MemberNames => _membersNames;


        private readonly ulong _value;
        private readonly string _name;

        private TENUM(ulong value, string name)
        {
            _value = value;
            _name = name;
        }


        public int CompareTo(TENUM other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(TENUM other)
        {
            throw new NotImplementedException();
        }

        public static TENUM Parse(string s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(string? s, IFormatProvider? provider, out TENUM result)
        {
            throw new NotImplementedException();
        }

        public static TENUM Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out TENUM result)
        {
            throw new NotImplementedException();
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            throw new NotImplementedException();
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }
    }


    public enum TestEnum
    {
        Default,
        Alpha,
        Beta,
        Gamma,
        Delta,
    }

    [Flags]
    public enum TestFlagsEnum
    {
        Default = 0,
        Alpha = 1 << 0,
        Beta = 1 << 1,
        Gamma = 1 << 2,
        Delta = 1 << 3,
    }


    public static class TempExtensions
    {
        // public static void AddFlag(this ref BindingFlags bindingFlags, BindingFlags flag)
        // {
        //     bindingFlags |= flag;
        // }
    }

/*
public static class Extensions
{
    public static int GetSetBitCount(long lValue)
    {
        int count = 0;

        //Loop the value while there are still bits
        while (lValue != 0)
        {
            //Remove the end bit
            lValue &= (lValue - 1);

            //Increment the count
            count++;
        }

        //Return the count
        return count;
    }

    public static int FlagCount(TestFlagsEnum testFlagsEnum)
    {
        return GetSetBitCount((long)testFlagsEnum);
    }
}
*/
}

#endif