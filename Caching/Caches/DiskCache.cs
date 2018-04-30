using System;
using System.Runtime.Caching;
using Amibou.Infrastructure.Configuration;

namespace Amibou.Infrastructure.Caching.Caches
{
    public class DiskCache : CacheBase
    {
        private FileCache _cache;
        public override CacheType CacheType => CacheType.Disk;

        protected override void InitialiseInternal()
        {
            if (_cache != null) return;

            _cache = new FileCache(CacheConfiguration.Current.DiskCache.Path)
            {
                MaxCacheSize = CacheConfiguration.Current.DiskCache.MaxSizeInMb
            };
        }

        protected override void SetInternal(string key, object value, TimeSpan validFor)
        {
            SetInternal(key, value);
        }

        protected override void SetInternal(string key, object value, DateTime expiresAt)
        {
            _cache.Set(key, value, expiresAt);
        }


        protected override void SetInternal(string key, object value)
        {
            _cache[key] = value;
        }

        protected override object GetInternal(string key) => _cache[key];

        protected override void RemoveInternal(string key)
        {
            _cache.Remove(key);
        }

        protected override bool ExistsInternal(string key) => _cache.Contains(key);

        protected override void ResetInternal()
        {
            _cache.Flush();
        }
    }
}
