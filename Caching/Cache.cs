using System;
using System.Linq;
using Amibou.Infrastructure.Caching.Caches;
using Amibou.Infrastructure.Configuration;
using Amibou.Infrastructure.Containers;

namespace Amibou.Infrastructure.Caching
{
    public static class Cache
    {
        public static ICache Get(CacheType cacheType)
        {
            ICache cache = new NullCache();
            try
            {
                var caches = Container.GetAll<ICache>();
                cache = (from c in caches
                         where c.CacheType == cacheType
                         select c).Last();
                cache.Initialise();
            }
            catch (Exception ex)
            {
                //Log.Warn("Failed to instantiate cache of type: {0}, using null cache. Exception: {1}", cacheType, ex);
                cache = new NullCache();
            }
            return cache;
        }

        public static ICache Default => Get(CacheConfiguration.Current.DefaultCacheType);

        public static ICache Memory => Get(CacheType.Memory);

        //public static ICache NCacheCache => Get(CacheType.NCache);

        public static ICache Disk => Get(CacheType.Disk);

        public static ICache Memcached => Get(CacheType.Memcached);

        public static void ResetDependentCache(string validationToken)
        {

        }
    }
}
