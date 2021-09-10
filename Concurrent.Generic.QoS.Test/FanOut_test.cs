using Concurrent.Generic.SoQ;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concurrent.Generic.QoS.Test
{
    class FanOut_test
    {
        Func<IChannel<int>> new_channel = () => new BufferedChannel<int>(1);

        Func<IChannel<int>, int, Func<int>> new_sender { get; set; }
        Func<IChannel<int>, Func<int>> new_reciver { get; set; }

        [SetUp]
        public void SetUp()
        {
            new_sender = new Func<IChannel<int>, int, Func<int>>((channel, max) =>
            {
                Random rand = new Random();
                return () =>
                {
                    int send = 0;
                    foreach (var item in System.Linq.Enumerable.Range(1, max))
                    {
                        int n = rand.Next(1, 10);
                        channel.In(n);
                        send += n;
                    }
                    return send;
                };
            });

            new_reciver = new Func<IChannel<int>, Func<int>>((channel) =>
            {
                return () => channel.Range().Aggregate(0, (a, b) => a + b);
            });
        }

        [Test]
        public void TestFanOut()
        {
            var channelIn = new_channel();

            int sendcount = 1 << 10;

            var fanout = new FanOut<int>(channelIn);

            var senders_count = 1;
            var recivers_count = 8;

            var channelOuts = Enumerable.Range(0, recivers_count).Select(x => fanout.NewChannelOut());

            IEnumerable<Task<int>> senders = Enumerable.Range(0, senders_count).Select(i => Task.Run(new_sender(channelIn, sendcount)));
            IEnumerable<Task<int>> recivers = channelOuts.Map(channel => Task.Run(new_reciver(channel)));


            int expected = 0;
            Task.Run(() =>
            {
                expected = senders.AsParallel()
                    .Select((t, seq) =>
                    {
                        var send = t.Result;
                        Console.WriteLine($"{DateTime.Now} Sender{seq}: {send}");
                        return send;
                    }).Aggregate(0, (a, b) => a + b);
                channelIn.Close();
            });

            //recivers = recivers.ToArray();

            fanout.Run();

            recivers.Map((seq, t) =>
            {
                var actuals = t.Result;
                Console.WriteLine($"{DateTime.Now} Reciver{seq}: {actuals}");
                Assert.AreEqual(expected, actuals);
                return actuals;
            });
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
