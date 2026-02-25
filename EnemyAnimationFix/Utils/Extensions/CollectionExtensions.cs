using System;
using System.Collections.Generic;

namespace EnemyAnimationFix.Utils.Extensions
{
    public static class CollectionExtensions
    {
        public static void Remove<T>(this List<T> list, Predicate<T> predicate)
        {
            int index;
            for (index = 0; index < list.Count; index++)
            {
                if (predicate(list[index]))
                {
                    list.RemoveAt(index);
                    return;
                }
            }
        }
    }
}
