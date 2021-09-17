using System;
using System.Collections.Generic;
using System.Linq;

namespace Concurrent.Generic
{
    public static class HigherOrder
    {
        //yield 사용하는 함수에서 IEnumerable<T>.ToArray() 호출하는 이유:
        //+즉시실행
        //+스텍오버플로우 방지

        /// <summary>
        /// Foreach
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        public static void Foreach<TSource>(this IEnumerable<TSource> source, Action<TSource> selector)
        {
            source.Foreach( selector_);

            bool selector_(int index, TSource source_)
            {
                selector(source_);
                return true;
            }
        }
        /// <summary>
        /// Foreach
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        public static void Foreach<TSource>(this IEnumerable<TSource> source, Action<int, TSource> selector)
        {
            source.Foreach(selector_);

            bool selector_(int index, TSource source_)
            {
                selector(index, source_);
                return true;
            }
        }
        /// <summary>
        /// Foreach with predict
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        public static void Foreach<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> selector)
        {
            source.Foreach( selector_);

            bool selector_(int index, TSource source_)
            {
                return selector(source_);
            }
        }
        /// <summary>
        /// Foreach with predict
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        public static void Foreach<TSource>(this IEnumerable<TSource> source, Func<int, TSource, bool> selector)
        {
            int index = 0;
            foreach (var e in source)
            {
                var pred = selector(index, e);
                if (false == pred)
                {
                    break;
                }
                index++;
            }
        }
        /// <summary>
        /// Map
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Map<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Map(selector_);

            TResult selector_(int index, TSource source_)
            {
                return selector(source_);
            }
        }
        /// <summary>
        /// Map
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Map<TSource, TResult>(this IEnumerable<TSource> source, Func<int, TSource, TResult> selector)
        {
            return map().ToArray();

            IEnumerable<TResult> map()
            {
                int index = 0;
                foreach (var e in source)
                {
                    yield return selector(index, e);
                    index++;
                }
            }
        }
        /// <summary>
        /// Fold
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="init"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TResult Fold<TSource, TResult>(this IEnumerable<TSource> source, TResult init, Func<TResult, TSource, TResult> selector)
        {
            return source.Fold(init, selector_);

            TResult selector_(int index, TResult result, TSource source_)
            {
                return selector(result, source_);
            }
        }
        /// <summary>
        /// Reduce
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="init"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TResult Fold<TSource, TResult>(this IEnumerable<TSource> source, TResult init, Func<int , TResult, TSource, TResult> selector)
        {
            TResult result = init;
            int index = 0;
            foreach (var e in source)
            {
                result = selector(index, result, e);
                index++;
            }
            return result;
        }
        /// <summary>
        /// Reduce
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Reduce<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return reduce().ToArray();

            IEnumerable<TResult> reduce()
            {
                foreach (var e in source)
                {
                    foreach (var r in selector(e))
                    {
                        yield return r;
                    }
                }
            }
        }
        /// <summary>
        /// Filter
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> Filter<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> selector)
        {
            return filter().ToArray();

            IEnumerable<TSource> filter()
            {
                foreach (var e in source)
                {
                    var pred = selector(e);
                    if (pred)
                    {
                        yield return e;
                    }
                }
            }
        }
        /// <summary>
        /// Append
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="lead"></param>
        /// <param name="follow"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> lead, IEnumerable<TSource> follow)
        {
            return append().ToArray();

            IEnumerable<TSource> append()
            {
                foreach (var e in lead)
                    yield return e;

                foreach (var e in follow)
                    yield return e;
            }
        }
        /// <summary>
        /// Append
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="lead"></param>
        /// <param name="follow"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> lead, params TSource[] follow)
        {
            return append().ToArray();

            IEnumerable<TSource> append()
            {
                foreach (var e in lead)
                    yield return e;

                foreach (var e in follow)
                    yield return e;
            }
        }
    }
}