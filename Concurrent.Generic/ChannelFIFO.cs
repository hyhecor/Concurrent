using System.Collections.Generic;
namespace Concurrent.Generic
{
    public sealed class ChannelFIFO<T> : IQueue<T>
    {
        LinkedList<T> FIFO { get; set; } = new LinkedList<T>();

        public int Count
        {
            get
            {
                lock (FIFO)
                {
                    return FIFO.Count;
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (FIFO)
            {
                FIFO.AddLast(item);
            }
        }

        public bool TryEnqueue(T item, int length)
        {
            lock (FIFO)
            {
                if (length <= FIFO.Count)
                    return false;

                FIFO.AddLast(item);
                return true;
            }
        }


        public bool TryDequeue(out T item)
        {
            lock (FIFO)
            {
                item = default(T);
                if (FIFO.First is null)
                    return false;

                item = FIFO.First.Value;
                FIFO.RemoveFirst();
                return true;
            }
        }

        public void Clear()
        {
            lock (FIFO)
            {
                FIFO.Clear();
            }
        }
    }
}