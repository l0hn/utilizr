using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilizr.Extensions
{
    public static class IEnumerableEx
    {
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            var pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(chunksize);
                pos += chunksize;
            }
        }

        /// <summary>
        /// Returns all elements that aren't distinct within the collection
        /// </summary>
        public static IEnumerable<T> NonDistinct<T>(this IEnumerable<T> source)
        {
            return NonDistinct(source, p => p);
        }

        /// <summary>
        /// Returns all elements that aren't distinct based on the keySelector
        /// </summary>
        public static IEnumerable<T> NonDistinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Where(p => p.Count() > 1).SelectMany(p => p);
        }

        public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> items, T separator)
        {
            var first = true;
            foreach (var item in items)
            {
                if (first) first = false;
                else
                {
                    yield return separator;
                }
                yield return item;
            }
        }

        //lazy init
        private static Random? _r;
        private static Random Rand => _r ??= new Random();

        /// <summary>
        /// Pick a random element from the enumerable
        /// </summary>
        public static T? Random<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            // note: creating a Random instance each call may not be correct for you,
            // consider a thread-safe static instance
            var list = enumerable as IList<T> ?? enumerable.ToList();
            return list.Count == 0 ? default(T) : list[Rand.Next(0, list.Count)];
        }

        /// <summary>
        /// Exposes ForEach on IEnumerable.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var t in collection)
            {
                action(t);
            }
        }

        /// <summary>
        /// Exposes ForEach on IEnumerable also adding index.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            int index = 0;
            foreach (var t in collection)
            {
                action(t, index);
                index++;
            }
        }
    }
}
