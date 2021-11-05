using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Generic
{
    public class Select : IDisposable
    {
        BlockList<Action> Closers { get; set; } = new BlockList<Action>();
        BlockList<Task> Tasks { get; set; } = new BlockList<Task>();

        BlockList<EventWaitHandle> EventWaitHandles { get; set; } = new BlockList<EventWaitHandle>();

        public void Dispose()
        {
            Close();
        }

        public Select()
        {
            init();
        }

        ~Select()
        {
            Close();

            while (false == (Tasks.Count(t => t.IsCompleted) == Tasks.Count()))
            {

            }
        }

        void init()
        {

        }

        public void Close()
        {
            Closers.Foreach(close => close());

            Closers.Clear();
        }

        /// <summary>
        /// 256 채널만 등록
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public Selector<T> Add<T>(IChannel<T> channel)
        {
            var OnSignalIn = NewEventWaitHandle();
            var OnSignalOut = NewEventWaitHandle();
            var id = Closers.Count();

            Closers.Add(channel.Close);
            EventWaitHandles.Add(OnSignalIn);

#if NET40
            var task = new Task(KeepReadChannel);
            task.Start();
            Tasks.Add(task); //백그라운드 실행
#elif NET45_OR_GREATER
            Tasks.Add(Task.Run(KeepReadChannel)); //백그라운드 실행
#else
            Tasks.Add(Task.Run(KeepReadChannel)); //백그라운드 실행
#endif
            T value = default;
            return new Selector<T>(id, () =>
            {
                var v = value; //get value 
                OnSignalOut.Set(); //after event set
                return v;
            });

            void KeepReadChannel()
            {
                channel.Range().Foreach(o =>
                {
                    OnSignalOut.Reset();
                    value = o;
                    OnSignalIn.Set(); //Set
                    OnSignalOut.WaitOne(); //Wait
                });
            }

            EventWaitHandle NewEventWaitHandle()
            {
                return new AutoResetEvent(false);
            }
        }

        /// <summary>
        /// Wait
        /// </summary>
        /// <param name="timeout">-1:Infinite</param>
        /// <returns>258:Timeout</returns>
        public int Wait(int timeout = -1)
        {
            var index = WaitHandle.WaitAny(EventWaitHandles.ToArray(), timeout);
#if false
            if (WaitHandle.WaitTimeout == index)
            {
                //타임아웃 
                //258
            }
#endif
            return index;
        }
    }

    public class Selector<T>
    {
        Func<T> GetValue { get; }

        public int Id { get; }
        public T Value { get => GetValue(); }
        public Selector(int id, Func<T> getter)
        {
            Id = id;
            GetValue = getter;
        }
    }
}