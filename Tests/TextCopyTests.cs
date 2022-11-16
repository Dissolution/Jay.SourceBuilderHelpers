using System.Runtime.CompilerServices;
using Benchmarks;
using Shouldly;

namespace Tests;

public class TextCopyTests
{
    public static IEnumerable<object[]> Arguments { get; }

    static TextCopyTests()
    {
        char[] GetChars()
        {
            var chars = new char[1024];
            chars.Initialize();
            return chars;
        }

        Arguments = new List<object[]>
        {
            new object[]{null!, GetChars()},
            new object[]{string.Empty, GetChars()},
            new object[]{",", GetChars() },
            new object[]{"\r\n", GetChars() },
            new object[]{Guid.NewGuid().ToString("N"), GetChars()},
            new object[]{new string('X', 1024), GetChars() },
        };
    }

    [Theory]
    [MemberData(nameof(Arguments))]
    public void StringCopyTo(string? source, char[] destination)
    {
        if (source is null) return;
        int len = source.Length;
        source?.CopyTo(0, destination, 0, len);
        destination[..len].ShouldBe<char>(source);
        destination[len..].ShouldAllBe(ch => ch == default);
    }

    [Theory]
    [MemberData(nameof(Arguments))]
    public void ArrayCopy(string? source, char[] destination)
    {
        if (source is null) return;
        int len = source.Length;
        Array.Copy(source.ToArray(), destination, len);
        destination[..len].ShouldBe<char>(source);
        destination[len..].ShouldAllBe(ch => ch == default);
    }

    [Theory]
    [MemberData(nameof(Arguments))]
    public unsafe void UnsafeCopyBlock(string? source, char[] destination)
    {
        if (source is null) return;
        int len = source.Length;
        fixed (char* sourcePtr = source)
        fixed (char* destinationPtr = destination)
        {
            Unsafe.CopyBlock(destinationPtr, sourcePtr, (uint)(len * sizeof(char)));
        }
        destination[..len].ShouldBe<char>(source);
        destination[len..].ShouldAllBe(ch => ch == default);
    }

    [Theory]
    [MemberData(nameof(Arguments))]
    public unsafe void BufferMemoryCopy(string? source, char[] destination)
    {
        if (source is null) return;
        int len = source.Length;
        fixed (char* sourcePtr = source)
        fixed (char* destinationPtr = destination)
        {
            Buffer.MemoryCopy(
                sourcePtr,
                destinationPtr,
                destination.Length * sizeof(char),
                len * sizeof(char));
        }
        destination[..len].ShouldBe<char>(source);
        destination[len..].ShouldAllBe(ch => ch == default);
    }

    /*
    [Theory]
    [MemberData(nameof(Arguments))]
    public void TextHelperCopyTo(string? source, char[] destination)
    {
        if (source is null) return;
        int len = source.Length;
        TextHelper.CopyTo(source[0], ref destination[0], len);
        destination[..len].ShouldBe<char>(source);
        destination[len..].ShouldAllBe(ch => ch == default);
    }
    */

    [Theory]
    [MemberData(nameof(Arguments))]
    public unsafe void TextHelperCopyToPtr(string? source, char[] destination)
    {
        if (source is null) return;
        int len = source.Length;
        fixed (char* sourcePtr = source)
        fixed (char* destinationPtr = destination)
        {
            TextHelper.CopyTo(sourcePtr, destinationPtr, len);
        }
        destination[..len].ShouldBe<char>(source);
        destination[len..].ShouldAllBe(ch => ch == default);
    }

}