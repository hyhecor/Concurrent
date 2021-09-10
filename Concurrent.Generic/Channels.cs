using System.Collections.Generic;

namespace Concurrent.Generic
{
    public static class Channels
    {
        public static IEnumerable<T> Range<T>(this IChannel<T> channel)
        {
            while (true)
            {
                var func = channel.Out();

                if (func is null)
                    yield break; //채널종료

                yield return func();
            }
        }

        public static void In<T>(this IChannel<T> channel, params T[] items)
        {
            items.Foreach((item) => channel.In(item));
        }
    }
}