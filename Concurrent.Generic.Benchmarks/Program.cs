using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;

namespace Concurrent.Generic.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Summary summary;
            summary = BenchmarkRunner.Run<FIFOBenchmarks>();
            summary = BenchmarkRunner.Run<ChannelBenchmarks>();
        }

    }
}
