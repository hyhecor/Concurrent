//#define CHANNEL_QUEUE
#if !CHANNEL_QUEUE
#define CHANNEL_FIFO
#endif
using System;
#if CHANNEL_QUEUE
using System.Collections.Concurrent;
#endif
using System.Threading;

namespace Concurrent.Generic
{
    /// <summary>
    /// Channel 
    /// (채널)
    /// <para>입력받은 데이터를 출력 함수에서 열거형 형식으로 반환 한다.</para> 
    /// <para></para> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Channel<T> : IChannel<T>, IDisposable
    {
        public void Dispose()
        {
            Close();
        }

#if CHANNEL_QUEUE
        ConcurrentQueue<T> Queue { get; set; } = new ConcurrentQueue<T>();
#endif
#if CHANNEL_FIFO
        ChannelFIFO<T> Queue { get; set; } = new ChannelFIFO<T>();
#endif

        EventWaitHandle OnClose { get; set; } = new ManualResetEvent(false);
        EventWaitHandle OnSignalIn { get; set; } = new AutoResetEvent(false);

        WaitHandle[] SignalIn { get; set; }

        public Channel()
        {
            init();
        }
       
        ~Channel()
        {
            Close();
        }

        void init()
        {
            SignalIn = new WaitHandle[] { OnClose, OnSignalIn };

            //설정 완료 후 클리어
            Clear();
        }

        public void Close()
        {
            OnClose.Set();
        }
        public void Clear()
        {
            Queue.Clear();

            OnClose.Reset();
            OnSignalIn.Reset();
        }

        public int Count { get => Queue.Count; }

        public void In(T item)
        {
            Queue.Enqueue(item);   //큐에 저장
            OnSignalIn.Set();      //입력 이벤트 SET
        }

        public Func<T> Out()
        {
            //입력 꺼내서 던지기
            T item;
            if (Queue.TryDequeue(out item))
            {
                return () => item;
            }
            else
            {
                var index = WaitHandle.WaitAny(SignalIn);
                //종료 이벤트
                if (0 == index)
                {
                    // 큐 카운트 확인
                    //큐에 데이터 있으면 꺼내서 던지고 다음 루프에서 종료
                    if (0 == Queue.Count)
                        return null;
                }
            }
            return Out();
        }
    }
}