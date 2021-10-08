using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Collections.Generic;

namespace Concurrent.Generic.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    public class FIFOBenchmarks
    {
        [Params(1)]
        public int N;

        static readonly Queue<int> queue = new Queue<int>();

        [Benchmark(Baseline = true)]
        public void Queue()
        {
            queue.Enqueue(N);
            int item;
            _ = queue.TryDequeue(out item);
        }

        static readonly FIFO<int> fifo = new FIFO<int>();

        [Benchmark]
        public void ChannelFIFO()
        {
            fifo.Enqueue(N);
            int item;
            _ = fifo.TryDequeue(out item);
        }
    }
}
