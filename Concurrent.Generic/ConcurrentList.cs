using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Concurrent.Generic
{
    public class ConcurrentList<T> : IEnumerable<T>
    {
        List<T> List { get; set; } = new List<T>();
        public T this[int index] { get => List[index]; set => List[index] = value; }

        public void Add(params T[] items)
        {
            lock (List)
            {
                foreach (var item in items)
                    List.Add(item);
            }
        }

        public bool Remove(params T[] items)
        {
            lock (List)
            {
                return items
                    .Select(x => List.Remove(x))
                    .Aggregate(true, (a, b) => a && b);
            }
        }

        public int IndexOf(T item)
        {
            lock (List)
            {
                return List.IndexOf(item);
            }
        }

        public void Clear()
        {
            lock (List)
            {
                List.Clear();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (List)
            {
                return List.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (List)
            {
                return List.GetEnumerator();
            }
        }
    }
}