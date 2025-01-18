using System;
using System.Collections.Generic;
namespace PCS.Observable
{
    public static class IListExtensions
    {
        public static void RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void AddRange<T>(this IList<T> list, IList<T> values)
        {
            foreach (var value in values)
                list.Add(value);
        }
    }
}