//#define CHANNEL_QUEUE
#if !CHANNEL_QUEUE
#define CHANNEL_FIFO
#endif
using System;
using System.Threading;
namespace Concurrent.Generic
{
    /// <summary>
    /// BufferedChannel
    /// (버퍼 크기 제한 채널) 
    /// <para>입력을 받을때 버퍼 카운트를 확인하여, </para> 
    /// <para>제한 크기보다 카운트가 작아야 입력을 받는다.</para> 
    /// <para>카운트가 크다면, 큐가 빠져나갈 때 까지 대기</para> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class BufferedChannel<T> : IChannel<T>, IDisposable
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
        EventWaitHandle OnSignalOut { get; set; } = new AutoResetEvent(true);

        WaitHandle[] SignlIn { get; set; }
        WaitHandle[] SignlOut { get; set; }

        int Length { get; set; } = 1;

        public BufferedChannel(int length = 1)
        {
            if (length < 0)
                length = 1;

            init(length);
        }
        ~BufferedChannel()
        {
            Close();
        }

        void init(int length)
        {
            Length = length;

            SignlIn = new WaitHandle[] { OnClose, OnSignalIn };
            SignlOut = new WaitHandle[] { OnClose, OnSignalOut };

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
            OnSignalOut.Set();
        }

        public int Count { get => Queue.Count; }
        public void In(T item)
        {
            //큐카운트 확인 
            //제한 값보다 작으면 큐에 저장
            if (Queue.TryEnqueue(item, Length))
            {
                OnSignalIn.Set();     //입력 이벤트 SET
                return;
            }
            //큐 크기를 넘으면 꺼내는 이벤트 기다리기
            else
            {
                int index = WaitHandle.WaitAny(SignlOut);
                if (0 == index)
                    return; //종료 이벤트 발생
            }

            In(item); //재귀
        }

        public Func<T> Out()
        {
            //입력 꺼내서 던지기
            T item;
            if (Queue.TryDequeue(out item))
            {
                OnSignalOut.Set();  //꺼내는 이벤트 SET
                return () => item;
            }
            else
            {
                var index = WaitHandle.WaitAny(SignlIn);
                //종료 이벤트
                if (0 == index)
                {
                    //큐 카운트 확인
                    //큐에 데이터 있으면 꺼내서 던지고 다음 루프에서 종료
                    if (0 == Queue.Count)
                        return null;
                }
            }
            return Out();
        }
    }
}