using System;
using System.Collections.Generic;

namespace Concurrent.Generic
{
    public static class Enumerables
    {
        public static void Foreach<T>(this IEnumerable<T> items, Action<T> func)
        {
            foreach (var item in items)
            {
                func(item);
            }
        }
        public static void Foreach<T>(this IEnumerable<T> items, Action<int, T> func)
        {
            int index = 0;
            foreach (var item in items)
            {
                func(index, item);
                index++;
            }
        }
        public static void Foreach<T>(this IEnumerable<T> items, Func<T, bool> func)
        {
            foreach (var item in items)
            {
                var pred = func(item);
                if (false == pred)
                {
                    break;
                }
            }
        }
        public static void Foreach<T>(this IEnumerable<T> items, Func<int, T, bool> func)
        {
            int index = 0;
            foreach (var item in items)
            {
                var pred = func(index, item);
                if (false == pred)
                {
                    break;
                }
                index++;
            }
        }
    }
}