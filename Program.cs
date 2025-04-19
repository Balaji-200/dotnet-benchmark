using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using LocationService.Benchmark;

static class Program
{
    static void Main() {
        var customConfig = ManualConfig
			.Create(DefaultConfig.Instance)
			.AddValidator(JitOptimizationsValidator.FailOnError)
			.AddDiagnoser(MemoryDiagnoser.Default)
			.AddColumn(StatisticColumn.AllStatistics)
			.AddJob(Job.Default.WithRuntime(CoreRuntime.Core60));

        var summary = BenchmarkRunner.Run<ReflectionBenchmark>(customConfig);
        Console.WriteLine(summary);
        // var reflect = new ReflectionBenchmark();
        // reflect.Benchmark();
    }
}