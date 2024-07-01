using CacheTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTests.Helpers
{
    public static class CacheTestUtilities
    {
         
        public static void GenerateCacheItems(int itemsToAdd)
        {
            for (int i = 1; i <= itemsToAdd; i++)
                Cache.Instance.AddOrUpdate("key" + i, "value" + i);
        }

        public static void GenerateIntCacheItems(int itemsToAdd)
        {
            for (int i = 1; i <= itemsToAdd; i++)
                Cache.Instance.AddOrUpdate("key" + i, i);
        }
    }
}
