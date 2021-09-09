using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Concurrent.Generic.SoQ
{
    /// <summary>
    /// QoS FanOut 구현
    /// </summary>
    public class FanOut<T>
    {
        IChannel<T> ChannelIn { get; set; }
        ConcurrentList<IChannel<T>> ChannelOuts { get; set; } = new ConcurrentList<IChannel<T>>();

        public FanOut(IChannel<T> channelIn)
        {
            ChannelIn = channelIn;
        }

        ~FanOut()
        {
            Close();
        }

        public void Close()
        {
            ChannelOuts.Foreach(c => c.Close());

            ChannelOuts.Clear();
        }

        public IChannel<T> NewChannelOut()
        {
            var newChannelOut = NewChannel();

            ChannelOuts.Add(newChannelOut);

            return newChannelOut;

            IChannel<T> NewChannel() => new BufferedChannel<T>(1);
        }

        public bool RemoveChannelOut(IChannel<T> channelOut)
        {
            return ChannelOuts.Remove(channelOut);
        }

        public void Run()
        {
            while (true)
            {
                var get_func = ChannelIn.Out();

                if (get_func is null)
                    break; //채널 종료됨

                var item = get_func();

                //Fan Out
                ChannelOuts.Foreach(channel => channel.In(item));
            }

            ChannelOuts.Foreach(c => c.Close());
        }
    }
}
