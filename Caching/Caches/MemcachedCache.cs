using Enyim.Caching;
using Enyim.Caching.Memcached;
using Amibou.Infrastructure.Logging;
using System;

namespace Amibou.Infrastructure.Caching.Caches
{
    public class MemcachedCache : CacheBase
    {
        private MemcachedClient _cache;

        public override CacheType CacheType => CacheType.Memcached;

        protected override void InitialiseInternal()
        {
            if (_cache == null)
            {
                //Log.Debug("MemcachedCache.Initialise - initialising");
                _cache = new MemcachedClient();
            }
        }

        protected override void SetInternal(string key, object value)
        {
            _cache.Store(StoreMode.Set, key, value);
        }

        protected override void SetInternal(string key, object value, DateTime expiresAt)
        {
            _cache.Store(StoreMode.Set, key, value, expiresAt);
        }

        protected override void SetInternal(string key, object value, TimeSpan validFor)
        {
            _cache.Store(StoreMode.Set, key, value, validFor);
        }

        protected override object GetInternal(string key) => _cache.Get(key);

        protected override void RemoveInternal(string key)
        {
            if (Exists(key))
            {
                _cache.Remove(key);
            }
        }

        protected override bool ExistsInternal(string key) => GetInternal(key) != null;

        protected override void ResetInternal()
        {
            _cache.FlushAll();
        }
    }
}
