﻿using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Jay.SourceGen.ConsoleApp;
using Jay.EntityGen.Attributes;
using Jay.SourceGen;
using Jay.SourceGen.Code;

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


var entityType = typeof(EntityBase);
var interfaces = entityType.GetInterfaces();
var members = entityType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
var entity = Activator.CreateInstance<EntityBase>();
entity.Name = "First";

var otherEntity = new EntityBase { Id = 0, Name = "joe" };

var eq = entity.Equals(otherEntity);
var eq2 = entity == otherEntity;


int c = entity.CompareTo(otherEntity);

string str = otherEntity.ToString();

//string strD = otherEntity.ToString("d");



Debugger.Break();

namespace Jay.SourceGen.ConsoleApp
{
    [Entity]
    public partial class EntityBase //: IEquatable<EntityBase>
    {
        [Key]
        public int Id { get; set; }

        [Key(KeyKind.Display)]
        public string Name { get; set; }

        [Key(KeyKind.Display)]
        public Guid Guid { get; set; } = Guid.NewGuid();
    }

    public class AnotherClass
    {

    }



    //[AsEnum]
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