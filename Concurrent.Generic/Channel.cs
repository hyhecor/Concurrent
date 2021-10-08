//#define CONCURRENT_QUEUE
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
    /// (버퍼 크기 제한 채널) 
    /// <para>입력을 받을때 버퍼 카운트를 확인하여, </para> 
    /// <para>제한 크기보다 카운트가 작아야 입력을 받는다.</para> 
    /// <para>카운트가 크다면, 큐가 빠져나갈 때 까지 대기</para> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Channel<T> : IChannel<T>, IDisposable
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

        ~Channel()
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
        EventWaitHandle OnSignalOut { get; set; } = new AutoResetEvent(true);

        WaitHandle[] SignlIn { get; set; }
        WaitHandle[] SignlOut { get; set; }

        int Length { get; set; } = 1;

        public Channel(int length = 1)
        {
            init(length);
        }

        void init(int length)
        {
            if (length < 0)
                length = 1;
            if (length == 0)
                length = 1;

            Length = length;

            SignlIn = new WaitHandle[] { OnClose, OnSignalIn, };
            SignlOut = new WaitHandle[] { OnClose, OnSignalOut, };

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
            OnSignalOut.Set();
        }

        public int Count { get => Queue.Count; }
        /// <summary>
        /// In (Blockable)
        /// </summary>
        /// <param name="item"></param>
        public void In(T item)
        {
            //STAT(0) 큐카운트 확인해서
            //제한 값보다 작으면 큐에 저장
            if (Queue.TryEnqueue(item, Length))
            {
                //STAT(1) 입력 성공
                //입력 이벤트 SET
                OnSignalIn.Set();

                //큐 크기가 큐 제한 크기와 비교하여 
                //수신자가 채널에서 꺼내갈 때까지 대기상태 설정
                while (Length == Count)
                {
                    Thread.Sleep(1);
                }

                return;
            }
            //큐 크기를 넘으면 꺼내는 이벤트 기다리기
            else
            {
                //STAT(2) 큐가 가득찼을 때 출력 기다리기
                int index = WaitHandle.WaitAny(SignlOut);
                //종료 이벤트(0) 확인 
                if (0 == index)
                    return; 
            }
            //STAT(3) 다시 시도
            //입력에 실패 했지만 채널이 종료되지 않음
            In(item);
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
                //꺼내는 이벤트 SET
                OnSignalOut.Set();  
                ////입력 리턴 이벤트 SET
                //OnSignalQueueOut.Set();
                //클로저 형태로 리턴
                return () => item;
            }
            else
            {
                //STAT(2) 큐가 비었을 때 입력 기다리기
                var index = WaitHandle.WaitAny(SignlIn);
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