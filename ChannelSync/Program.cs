using Concurrent.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChannelSync
{
    class Program
    {
        static void Main(string[] args)
        {
            Program app = new Program();
            var result = app.Run();

            Environment.Exit(result ? 0 : -1);
        }

        bool Run()
        {
            IChannel<int> channel_in = new Channel<int>();
            IChannel<int> channel_out = new Channel<int>();

            Task.Run(Reciver(channel_in, channel_out));

            var send_count = 1 << 7;

            LogWrite($"Send 0 to {send_count}");

            var sum = 0;
            foreach (var value in Enumerable.Range(0, send_count))
            {
                sum += value;
                channel_in.In(value);
            }
            channel_in.Close();

            return channel_out.Out()() == sum;
        }

        Action Reciver(IChannel<int> channel_in, IChannel<int> channel_out)
        {
            return ()=> 
            {
                LogWrite($"reciver start");

                LogWrite($"Delay");
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();

                LogWrite($"consume");

                var sum = 0;
                foreach (var value in channel_in.Range())
                {
                    sum += value;
                    LogWrite($"Recv({value})");
                }

                channel_out.In(sum);

                LogWrite($"reciver closed");
            };
        }

        void LogWrite(string msg)
        {
            Console.WriteLine($"{DateTime.Now} {msg}");
        }
    }
}
