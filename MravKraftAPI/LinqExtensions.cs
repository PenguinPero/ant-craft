using System;
using System.Collections.Generic;
using System.Linq;

namespace MravKraftAPI
{
    public static class LINQExtensions
    {
        public static TSource MinBy<TSource, TProp>(this IEnumerable<TSource> source, Func<TSource, TProp> keySelector)
            where TProp : IComparable<TProp>
        {
            if (!source.Any()) return default(TSource);

            TSource minElem = source.First();
            TProp minValue = keySelector(minElem);

            foreach (TSource elem in source)
                if (keySelector(elem).CompareTo(minValue) < 0)
                {
                    minValue = keySelector(elem);
                    minElem = elem;
                }

            return minElem;
        }
    }
}
