using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Collections.Generic;

namespace Concurrent.Generic.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    public class ChannelBenchmarks
    {
        [Params(1000, 10000)]
        public int N;

        static readonly Channel<int> channel = new Channel<int>();

        [Benchmark]
        public void Channel()
        {
            channel.In(N);
            var getter = channel.Out();
            if (getter is object)
            {
                int item = getter();
            }
        }

        static readonly BufferedChannel<int> bufferedchannel = new BufferedChannel<int>();

        [Benchmark]
        public void BufferedChannel()
        {
            int item = 0;
            bufferedchannel.In(item);
            var getter = bufferedchannel.Out();
            if (getter is object)
            {
                item = getter();
            }
        }

        static readonly Queue<int> queue = new Queue<int>();

        [Benchmark(Baseline = true)]
        public void Queue()
        {
            queue.Enqueue(N);
            int item;
            _ = queue.TryDequeue(out item);
        }

        static readonly ChannelFIFO<int> fifo = new ChannelFIFO<int>();

        [Benchmark]
        public void ChannelFIFO()
        {
            fifo.Enqueue(N);
            int item;
            _ = fifo.TryDequeue(out item);
        }


    }
}
