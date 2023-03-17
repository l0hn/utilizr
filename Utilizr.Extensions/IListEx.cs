using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilizr.Extensions
{
    public static class IListEx
    {
        public static IList<T> Clone<T>(this IList<T> clonableList) where T : ICloneable
        {
            return clonableList.Select(p => (T)p.Clone()).ToList();
        }

        /// <summary>
        /// Safe index for looped collections where first and last items appear next to each other.
        /// The remainder is always used when the unwrappedIndex is bigger/smaller than a multiple of the collection size.
        /// E.g. -1 will return index of last item.
        /// Last index + 1 will return first item index (0).
        public static int WrappedIndex<T>(this IList<T> list, int unwrappedIndex)
        {
            var size = list.Count;
            return ((unwrappedIndex % size) + size) % size;
        }
    }
}