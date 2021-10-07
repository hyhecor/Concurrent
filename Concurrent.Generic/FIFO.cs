using System.Collections.Generic;
namespace Concurrent.Generic
{
    internal sealed class FIFO<T> : IQueue<T>
    {
        LinkedList<T> LList { get; set; } = new LinkedList<T>();

        public int Count
        {
            get
            {
                lock (LList)
                {
                    return LList.Count;
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (LList)
            {
                LList.AddLast(item);
            }
        }

        public bool TryEnqueue(T item, int max)
        {
            lock (LList)
            {
                if (max <= LList.Count)
                    return false;

                LList.AddLast(item);
                return true;
            }
        }


        public bool TryDequeue(out T item)
        {
            lock (LList)
            {
                item = default(T);
                if (LList.First is null)
                    return false;

                item = LList.First.Value;
                LList.RemoveFirst();
                return true;
            }
        }

        public void Clear()
        {
            lock (LList)
            {
                LList.Clear();
            }
        }
    }
}