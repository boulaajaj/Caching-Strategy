
namespace Amibou.Infrastructure.Caching
{
    public enum CacheType
    {
        /// <summary>
        /// No cache type set
        /// </summary>
        Null = 0,
        
        Memory,

        Memcached,

        Disk,
        //!!!!!NCache isn't done yet!!!!!
        NCache
    }
}
