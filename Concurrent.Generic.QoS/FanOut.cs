using System.Linq;
using System.Threading.Tasks;

namespace Concurrent.Generic.SoQ
{
    /// <summary>
    /// QoS FanOut 구현
    /// </summary>
    public class FanOut<T>
    {
        IChannel<T> ChannelIn { get; set; }
        BlockList<IChannel<T>> ChannelOuts { get; set; } = new BlockList<IChannel<T>>();

        BlockList<Task> Tasks { get; set; } = new BlockList<Task>();
     
        public FanOut(IChannel<T> channelIn)
        {
            ChannelIn = channelIn;

#if NET40
            var task = new Task(FanOutWork);
            task.Start();
            Tasks.Add(task); //백그라운드 실행
#elif NET45_OR_GREATER
            Tasks.Add(Task.Run(FanOutWork)); //백그라운드 실행
#else
            Tasks.Add(Task.Run(FanOutWork)); //백그라운드 실행
#endif

            void FanOutWork()
            {
                ChannelIn.Range().Foreach(o => ChannelOuts.Foreach(c => c.In(o)));

                Close();
            }
        }

        ~FanOut()
        {
            Close();

            while (false == (Tasks.Count(t => t.IsCompleted) == Tasks.Count()))
            {

            }
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

            IChannel<T> NewChannel() => new NBChannel<T>();

        }

        public bool RemoveChannelOut(IChannel<T> channelOut)
        {
            return ChannelOuts.Remove(channelOut);
        }
    }
}
