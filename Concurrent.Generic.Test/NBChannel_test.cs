using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
#if NET35
using System.Threading;
#else
using System.Threading.Tasks;
#endif

namespace Concurrent.Generic.Test
{
    class NBChannel_test
    {
        Func<IChannel<int>> new_channel = () => new NBChannel<int>();

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
        public void TestNBChannel()
        {
            var channel = new_channel();

            int sendcount = 1 << 10;
#if NET35
            var task = new Thread(new_sender(channel, sendcount));
#else
            var task = Task.Run(new_sender(channel, sendcount));
#endif
            int expected = 0;
            Task.Run(() =>
            {
                expected = task.Result;
                channel.Close();
                Console.WriteLine($"{DateTime.Now} Sender: {expected}");
            });
            int actual = 0;
            foreach (var item in channel.Range())
            {
                actual += item;
                //Task.Delay(3).Wait(); //all day long
            }

            Console.WriteLine($"{DateTime.Now} {expected} {actual}");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestNBChannelLinq()
        {
            int sendcount = 1 << 10;
            var channel = new_channel();
            var task = Task.Run(new_sender(channel, sendcount));

            int expected = 0;
            Task.Run(() =>
            {
                expected = task.Result;
                channel.Close();
                Console.WriteLine($"{DateTime.Now} Sender: {expected}");
            });

            int actual = channel.Range().Aggregate(0, (a, b) => a + b);

            Console.WriteLine($"{DateTime.Now} {expected} {actual}");
            Assert.AreEqual(expected, actual);

            task.Wait();
        }

        [Test]
        public void TestNBChannelLinqFanin()
        {
            var channel = new_channel();

            int sendcount = 1 << 10;
            var senders_count = 16;
            var recivers_count = 1;
            IEnumerable<Task<int>> senders = Enumerable.Range(0, senders_count).Select(i => Task.Run(new_sender(channel, sendcount))); 
            IEnumerable<Task<int>> recivers = Enumerable.Range(0, recivers_count).Select(i => Task.Run(new_reciver(channel)));

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
                channel.Close();
            });

            var actual = recivers.AsParallel()
                .Select((t, seq) =>
                {
                    var recive = t.Result;
                    Console.WriteLine($"{DateTime.Now} Reciver{seq}: {recive}");
                    return recive;
                }).Aggregate(0, (a, b) => a + b);

            Console.WriteLine($"{DateTime.Now} {expected } {actual}");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestNBChannelLinqRace()
        {
            var channel = new_channel();

            int sendcount = 1 << 10;
            var senders_count = 1;
            var recivers_count = 16;
            IEnumerable<Task<int>> senders = Enumerable.Range(0, senders_count).Select(i => Task.Run(new_sender(channel, sendcount)));
            IEnumerable<Task<int>> recivers = Enumerable.Range(0, recivers_count).Select(i => Task.Run(new_reciver(channel)));

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
                channel.Close();
            });

            var actual = recivers.AsParallel()
                .Select((t, seq) =>
                {
                    var recive = t.Result;
                    Console.WriteLine($"{DateTime.Now} Reciver{seq}: {recive}");
                    return recive;
                }).Aggregate(0, (a, b) => a + b);

            Console.WriteLine($"{DateTime.Now} {expected } {actual}");
            Assert.AreEqual(expected, actual);
        }

        [TearDown]
        public void TearDown()
        {
        }

    }
}
