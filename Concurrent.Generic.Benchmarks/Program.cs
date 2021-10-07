using BenchmarkDotNet.Running;
using System;

namespace Concurrent.Generic.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var summary = BenchmarkRunner.Run<ChannelBenchmarks>();
        }

    }
}
