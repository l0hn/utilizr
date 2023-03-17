using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Utilizr.Extensions
{
    public static class ObservableCollectionEx
    {
        /// <summary>
        /// Remember that an event will fire for every added item!
        /// </summary>
        public static void AddRange<T>(this ObservableCollection<T> source, params T[] items)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }

        /// <summary>
        /// Remember that an event will fire for every added item!
        /// </summary>
        public static void AddRange<T>(this ObservableCollection<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }

        /// <summary>
        /// Remember that an event will fire for every inserted item. Items inserted sequentially.
        /// </summary>
        public static void InsertRange<T>(this ObservableCollection<T> source, int index, params T[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                source.Insert(index + i, items[i]);
            }
        }

        /// <summary>
        /// Remember that an event will fire for every inserted item. Items inserted sequentially.
        /// </summary>
        public static void InsertRange<T>(this ObservableCollection<T> source, int index, IEnumerable<T> items)
        {
            InsertRange(source, index, items.ToArray());
        }


        /// <summary>
        /// Remember that an event will fire for every inserted item. Items inserted sequentially.
        /// </summary>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
        {
            var obsCollection = new ObservableCollection<T>();
            obsCollection.AddRange(collection);
            return obsCollection;
        }
    }
}
