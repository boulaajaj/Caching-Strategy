using Amibou.Infrastructure.Caching;

namespace Amibou.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions to <see cref="ICache"/>
    /// </summary>
    public static class CacheExtensions
    {
        public static T Get<T>(this ICache cache, string key) where T : class
            => cache.Get(key) as T;

    }
}
