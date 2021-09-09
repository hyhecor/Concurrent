using System;
using System.Collections.Generic;

namespace Concurrent.Generic
{
    /// <summary>
    /// 채널 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannel<T> : IDisposable
    {
        /// <summary>
        /// 채널 종료
        /// </summary>
        void Close();
        /// <summary>
        /// 클리어 채널
        /// </summary>
        void Clear();
        /// <summary>
        /// 큐 카운트
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 채널 입구
        /// </summary>
        void In(T item);
        /// <summary>
        /// 채널 출구
        /// <para>리턴 값이 없으면 종료된 채널</para>
        /// </summary>
        Func<T> Out();
    }
}