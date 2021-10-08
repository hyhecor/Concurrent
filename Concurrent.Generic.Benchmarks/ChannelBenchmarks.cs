using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Concurrent.Generic.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    public class ChannelBenchmarks
    {
        [Params(1)]
        public int N;

        static readonly NBChannel<int> nbchannel = new NBChannel<int>();

        [Benchmark(Baseline = true)]
        public void NBChannel()
        {
            nbchannel.In(N);
            var getter = nbchannel.Out();
            if (getter is object)
            {
                int item = getter();
            }
        }

        static readonly Channel<int> channel = new Channel<int>(2);

        [Benchmark]
        public void Channel()
        {
            int item = 0;
            channel.In(item);
            var getter = channel.Out();
            if (getter is object)
            {
                item = getter();
            }
        }
    }
}
