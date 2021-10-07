
namespace Concurrent.Generic
{
    /// <summary> 
    /// 큐 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueue<T>
    {
        /// <summary>
        /// 큐 카운트
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 큐에 넣기
        /// </summary>
        /// <param name="item"></param>
        void Enqueue(T item);
        /// <summary>
        /// 큐 넣기; 큐 카운트에 제한되는 큐 넣기
        /// </summary>
        /// <param name="item"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        bool TryEnqueue(T item, int max);
        /// <summary>
        /// 큐에서 빼기 시도
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TryDequeue(out T item);
        /// <summary>
        /// 큐 클리어
        /// </summary>
        void Clear();
    }
}