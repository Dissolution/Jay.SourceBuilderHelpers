using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Benchmarks;
using Jay.SourceBuilderHelpers.Text;

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


using var codeWriter = new CodeWriter();




















#endif

