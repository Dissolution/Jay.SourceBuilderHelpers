using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

// [InProcess]
// [ShortRunJob]
public class TextCopyBenchmarks
{
    public IEnumerable<object[]> Arguments { get; }

    public TextCopyBenchmarks()
    {
        var chars = new char[1024];

        this.Arguments = new List<object[]>
        {
            //new object[2]{null!, chars},
            //new object[2]{string.Empty, chars},
            //new object[2]{",", chars},
            new object[2]{"\r\n", chars},
            new object[2]{Guid.NewGuid().ToString("N"), chars},
            new object[2]{new string('X', 1024), chars},
        };
    }

    [Benchmark]
    [ArgumentsSource(nameof(Arguments))]
    public void StringCopyTo(string? source, char[] destination)
    {
        source?.CopyTo(0, destination, 0, source.Length);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Arguments))]
    public void ArrayCopy(string? source, char[] destination)
    {
        if (source is not null)
        {
            Array.Copy(source.ToArray(), destination, source.Length);
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(Arguments))]
    public unsafe void UnsafeCopyBlock(string? source, char[] destination)
    {
        if (source is not null)
        {
            fixed (char* sourcePtr = source)
            fixed (char* destinationPtr = destination)
            {
                Unsafe.CopyBlock(destinationPtr, sourcePtr, (uint)source.Length*sizeof(char));
            }
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(Arguments))]
    public unsafe void BufferMemoryCopy(string? source, char[] destination)
    {
        if (source is not null)
        {
            fixed (char* sourcePtr = source)
            fixed (char* destinationPtr = destination)
            {
                Buffer.MemoryCopy(
                    sourcePtr, 
                    destinationPtr,
                    destination.Length * sizeof(char),
                    source.Length * sizeof(char));
            }
        }
    }

    /*
    [Benchmark]
    [ArgumentsSource(nameof(Arguments))]
    public void TextHelperCopyTo(string? source, char[] destination)
    {
        if (source is not null)
        {
            TextHelper.CopyTo(source[0], ref destination[0], source.Length);
        }
    }
    */

    [Benchmark]
    [ArgumentsSource(nameof(Arguments))]
    public unsafe void TextHelperCopyToPtr(string? source, char[] destination)
    {
        if (source is not null)
        {
            fixed (char* sourcePtr = source)
            fixed (char* destinationPtr = destination)
            {
                TextHelper.CopyTo(sourcePtr, destinationPtr, source.Length);
            }
        }
    }

}