#define CONCURRENT_QUEUE
#if !CONCURRENT_QUEUE
#define FIFO
#endif
using System;
#if CONCURRENT_QUEUE
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
    public sealed class NoneBlockChannel<T> : IChannel<T>, IDisposable
    {
        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                Close();
            }
            disposedValue = true;
        }

        ~NoneBlockChannel()
        {
            Dispose(disposing: false);
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

#if CONCURRENT_QUEUE
        ConcurrentQueue<T> Queue { get; set; } = new ConcurrentQueue<T>();
#endif
#if FIFO
        FIFO<T> Queue { get; set; } = new FIFO<T>();
#endif

        EventWaitHandle OnClose { get; set; } = new ManualResetEvent(false);
        EventWaitHandle OnSignalIn { get; set; } = new AutoResetEvent(false);

        WaitHandle[] SignalIn { get; set; }

        public NoneBlockChannel()
        {
            init();
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
#if FIFO
            Queue.Clear();
#endif
            OnClose.Reset();
            OnSignalIn.Reset();
        }

        public int Count { get => Queue.Count; }

        /// <summary>
        /// None block In
        /// </summary>
        /// <param name="item"></param>
        public void In(T item)
        {
            //큐에 저장
            Queue.Enqueue(item);
            //입력 이벤트 SET
            OnSignalIn.Set();
        }

        /// <summary>
        /// Out
        /// </summary>
        /// <returns></returns>
        public Func<T> Out()
        {
            //STAT(0) 큐에서 꺼내기 시도
            T item;
            if (Queue.TryDequeue(out item))
            {
                //STAT(1) 큐에서 성공적으로 입력 값을 꺼냈음
                //클로저 형태로 리턴
                return () => item;
            }
            else
            {
                //STAT(2) 큐가 비었을 때 입력 기다리기
                var index = WaitHandle.WaitAny(SignalIn);
                //종료 이벤트(0) 확인 
                if (0 == index)
                {
                    //큐 카운트 확인
                    //큐에 데이터 없으면 리턴(NULL)
                    //큐에 데이터가 있으면 다시 시도
                    if (0 == Queue.Count)
                        return null;
                }
            }
            //STAT(3) 다시 시도
            //큐가 비어있어서 상태에서 입력 기다리다 
            //입력 이벤트 발생 
            //혹은 채널 종료 이벤트가 발생 하였지만 
            //큐에 데이터가 있어서 다시 꺼내기 시도
            return Out();
        }
    }
}