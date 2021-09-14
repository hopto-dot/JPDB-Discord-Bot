using System;
using System.Collections.Generic;

namespace JPDB_Bot
{
    public static class Utility
    {
        public static T ChooseRandomItem<T>(Random random, IReadOnlyList<T> items)
        {
            return items.Count == 0 ? default(T) : items[random.Next(items.Count)];
        }
    }
}