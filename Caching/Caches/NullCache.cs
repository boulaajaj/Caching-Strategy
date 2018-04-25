using System;

namespace Amibou.Infrastructure.Caching.Caches
{
    /// <summary>
    /// <see cref="ICache"/> implementation which does nothing
    /// </summary>
    /// <remarks>
    /// Used when real caches are unavailable or disabled
    /// </remarks>
    public  class NullCache : CacheBase
    {

        public override CacheType CacheType => CacheType.Null;

        protected override void InitialiseInternal() { }

        protected override void SetInternal(string key, object value) { }

        protected override void SetInternal(string key, object value, DateTime expiresAt) { }

        protected override void SetInternal(string key, object value, TimeSpan validFor) { }

        protected override object GetInternal(string key) => null;

        protected override void RemoveInternal(string key) { }

        protected override bool ExistsInternal(string key) => false;

        protected override void ResetInternal() { }
    }
}
