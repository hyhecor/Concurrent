using System;
using System.Collections.Generic;

namespace Concurrent.Generic
{
    public static class Channels
    {
        //public static void Foreach<T>(this IConcurrentGenericChannel<T> channel, Func<T, bool> func)
        //{
        //    foreach (var item in channel.Out())
        //    {
        //        var pred = func(item);
        //        if (false == pred)
        //        {
        //            channel.Close(); //채널 종료
        //            break;
        //        }
        //    }
        //}
        public static IEnumerable<T> Range<T>(this IChannel<T> channel)
        {
            while (true)
            {
                var func = channel.Out();

                if (func is null)
                    yield break;

                yield return func();
            }
        }

        public static void In<T>(this IChannel<T> channel, params T[] items)
        {
            items.Foreach((item) => channel.In(item));
        }


    }
}