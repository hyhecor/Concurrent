using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Generic
{
    public class Select : IDisposable
    {
        ConcurrentList<Action> Closers { get; set; } = new ConcurrentList<Action>();
        ConcurrentList<Task> Tasks { get; set; } = new ConcurrentList<Task>();

        ConcurrentList<EventWaitHandle> EventWaitHandles { get; set; } = new ConcurrentList<EventWaitHandle>();

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
        }

        void init()
        {

        }

        public void Close()
        {
            Closers.Foreach(close => close());

            Tasks.Foreach(t => t.Wait());
        }

        /// <summary>
        /// 256 채널만 등록
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public Func<T> Add<T>(IChannel<T> channel)
        {
            var OnSignalIn = NewEventWaitHandle();
            var OnSignalOut = NewEventWaitHandle();

            Closers.Add(channel.Close);
            EventWaitHandles.Add(OnSignalIn);
            Tasks.Add(Task.Run(KeepReadChannel)); //백그라운드 실행

            T value = default;
            return () =>
            {
                var v = value; //get value 
                OnSignalOut.Set(); //after event set
                return v;
            };

            void KeepReadChannel()
            {
                while (true)
                {
                    var get_func = channel.Out();

                    //채널 종료
                    if (get_func is null)
                        return; 

                    value = get_func();
                    OnSignalIn.Set(); //Set
                    OnSignalOut.WaitOne(); //Wait
                }
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
}