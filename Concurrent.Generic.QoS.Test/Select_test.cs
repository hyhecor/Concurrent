using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Generic.QoS.Test
{
    class Select_test
    {

        Func<IChannel<int>> new_channel = () => new Channel<int>(1);

        Func<IChannel<int>, int, Func<int>> new_sender { get; set; }

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

                        //int n = send;
                        //channel.In(n);
                        //send += 1;
                    }
                    return send;
                };
            });

        }

        [Test]
        public void TestSelectTypes()
        {
            var rangemax = 3;

            Channel<object> channelclose = new Channel<object>();
            Channel<int> channel0 = new Channel<int>();
            Channel<string> channel1 = new Channel<string>();
            Channel<bool> channel2 = new Channel<bool>();

            //Task.Delay(1000).ContinueWith((t) => channelclose.In(true));
            var t0 = Task.Run((Action)(() =>
            {
                Enumerable.Range(0, rangemax).Foreach((int i) => channel0.In(i));
                channel0.Close();
            }));
            var t1 = Task.Run((Action)(() =>
            {
                Enumerable.Range(0, rangemax).Foreach((int i) => channel1.In($"Index:{i}"));
                channel1.Close();
            }));
            var t2 = Task.Run((Action)(() =>
            {
                Enumerable.Range(0, rangemax).Foreach((int i) => channel2.In(0 == i % 2));
                channel2.Close();
            }));


            Task.Run(() =>
            {
                t0.Wait();
                t1.Wait();
                t2.Wait();

                channelclose.In(true);
            });


            var select = new Select();
            var selector0 = select.Add(channel0);
            var selector1 = select.Add(channel1);
            var selector2 = select.Add(channel2);
            var closer = select.Add(channelclose);

            bool run = true;
            while (run)
            {
                //switch(select.Wait())
                //{
                //    case 0:
                //        Console.WriteLine($"channel0: {get_value0()}");
                //        break;
                //    case 1:
                //        Console.WriteLine($"channel0: {get_value1()}");
                //        break;
                //    case 2:
                //        Console.WriteLine($"channel0: {get_value2()}");
                //        break;
                //    case 3:
                //        Console.WriteLine($"channel0: {get_close()}");
                //        run = false;
                //        break;
                //    case WaitHandle.WaitTimeout:
                //        break;
                //    default:
                //        run = false;
                //        break;
                //}
                var id = select.Wait();
                if (selector0.Id == id)
                {
                    Console.WriteLine($"selector0: {selector0.Value}");
                }
                else if (selector1.Id == id)
                {
                    Console.WriteLine($"selector1: {selector1.Value}");
                }
                else if (selector2.Id == id)
                {
                    Console.WriteLine($"selector2: {selector2.Value}");
                }
                else if (closer.Id == id)
                {
                    Console.WriteLine($"closer: {closer.Value}");
                    run = false;
                }
                else if (WaitHandle.WaitTimeout == id)
                {
                    Console.WriteLine($"timeout");
                    run = false;
                }
            }
        }

        [Test]
        public void TestSelectSimple()
        {
            var channel_0 = new_channel();
            var channel_1 = new_channel();
            var channel_close = new_channel();

            int sendcount = 1 << 10;


            int expected_0 = 0;
            var t0 = Task.Run(() =>
            {
                expected_0 = Task.Run(new_sender(channel_0, sendcount)).Result;
                channel_0.Close();
                Console.WriteLine($"{DateTime.Now} Sender 0: {expected_0}");
            });

            int expected_1 = 0;
            var t1= Task.Run(() =>
            {
                expected_1 = Task.Run(new_sender(channel_1, sendcount)).Result;
                channel_1.Close();
                Console.WriteLine($"{DateTime.Now} Sender 1: {expected_1}");
            });

            Task.Run(() => 
            {
                t0.Wait();
                t1.Wait();

                channel_close.In(1);
            });

            int actual_0 = 0;
            int actual_1 = 0;

            var select = new Select();
            var selector0 = select.Add(channel_0);
            var selector1 = select.Add(channel_1);
            var closer = select.Add(channel_close);

            bool run = true;
            while (run)
            {
                switch (select.Wait())
                {
                    case 0:
                        var value0 = selector0.Value;
                        actual_0 += value0;
                        break;
                    case 1:
                        var value1 = selector1.Value;
                        actual_1 += value1;
                        break;
                    case 2:
                        var value2 = closer.Value;
                        run = false;
                        break;
                    case WaitHandle.WaitTimeout:
                        run = false;
                        break;
                    default:
                        break;
                }
            }

            //select 동작은 값이 보장되지 않음 
            //중간에 종료되면 채널에 넣은 데이터를 중간에 가져오지 않는다.
            Console.WriteLine($"{DateTime.Now} {expected_0} {actual_0}");
            Console.WriteLine($"{DateTime.Now} {expected_1} {actual_1}");
            Assert.AreEqual(expected_0, actual_0);
            Assert.AreEqual(expected_1, actual_1);
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
