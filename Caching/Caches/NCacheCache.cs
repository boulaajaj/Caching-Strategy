using System;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Web.Caching;
using Amibou.Infrastructure.Configuration;
using Amibou.Infrastructure.Logging;
using System.Linq;

namespace Amibou.Infrastructure.Caching.Caches
{
    /// <summary>
    /// <see cref="ICache"/> implementation using NCacheCache as the backing cache
    /// </summary>
    /// <remarks>
    /// Uses CacheConfiguration setting "defaultCacheName" to determine the cache name.
    /// Defaults to "Amibou.Infrastructure.Cache" if not set
    /// </remarks>
    public class NCacheCache : CacheBase //!!!!!NCache isn't done yet!!!!!
    {
        public DateTime NoAbsoluteExpiry { get; set;}
        public TimeSpan NoSlidingExpiry { get; set; }
        private string _cacheName;

        /// <summary>
        /// Returns the cache type
        /// </summary>
        public override CacheType CacheType => CacheType.NCache; //!!!!!NCache isn't done yet!!!!!

        protected override void InitialiseInternal()
        {
            if (_cacheName == null)
            {
                _cacheName = CacheConfiguration.Current.DefaultCacheName;
                //Log.Debug("NCacheCache.Initialise - initialising with cacheName: {0}", _cacheName);
                NoAbsoluteExpiry = DateTime.MaxValue;
                NoSlidingExpiry = new TimeSpan(0);
                NCache.InitializeCache(_cacheName);
            }
        }

        protected override void SetInternal(string key, object value)
        {
            NCache.Caches[_cacheName].Insert(key, value, null, NoAbsoluteExpiry, NoSlidingExpiry, CacheItemPriority.Normal);            
        }

        protected override void SetInternal(string key, object value, DateTime expiresAt)
        {
            NCache.Caches[_cacheName].Insert(key, value, null, expiresAt, NoSlidingExpiry, CacheItemPriority.Normal);
        }

        protected override void SetInternal(string key, object value, TimeSpan validFor)
        {
            var expiresAt = DateTime.UtcNow.Add(validFor);
            SetInternal(key, value, expiresAt);            
        }

        protected override object GetInternal(string key) => NCache.Caches[_cacheName].Get(key);

        protected override void RemoveInternal(string key)
        {
            NCache.Caches[_cacheName].Remove(key);
        }

        protected override bool ExistsInternal(string key) => NCache.Caches[_cacheName].Get(key) != null;

        protected override void ResetInternal()
        {
            NCache.Caches[_cacheName].Clear();
        }
    }
}
