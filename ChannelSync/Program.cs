using Concurrent.Generic;
using System;
using System.Threading.Tasks;

namespace ChannelSync
{
    class Program
    {
        static void Main(string[] args)
        {
            IChannel<int> channel = new Channel<int>(2);

            Task.Run(()=>
            {
                Console.WriteLine($"{DateTime.Now} Delay");
                Task.Delay(TimeSpan.FromSeconds(5)).Wait();

                Console.WriteLine($"{DateTime.Now} demand");
                var recv = channel.Out();
                Console.WriteLine($"{DateTime.Now} gain");
            });

            var send = 0;

            Console.WriteLine($"{DateTime.Now} Send");
            channel.In(send);
            channel.In(send);
        }
    }
}
