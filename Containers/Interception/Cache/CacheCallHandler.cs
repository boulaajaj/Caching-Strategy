using Amibou.Infrastructure.Caching;
using Amibou.Infrastructure.Configuration;
using Amibou.Infrastructure.Instrumentation;
using Amibou.Infrastructure.Instrumentation.PerformanceCounters;
using Amibou.Infrastructure.Serialization;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Linq;
using System.Reflection;
using static Amibou.Infrastructure.Caching.CacheValidationHandler;

namespace Amibou.Infrastructure.Containers.Interception.Cache
{
    /// <summary>
    /// <see cref="ICallHandler"/> for intercepting method calls and caching the responses
    /// </summary>
    public class CacheCallHandler : ICallHandler
    {
        /// <summary>
        /// Order which the handler should fire
        /// </summary>
        public int Order { get; set; }

        public static bool CachesWereReset { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously cached response or invokes method and caches response
        /// </summary>
        /// <param name="method"></param>
        /// <param name="getNext"></param>
        /// <returns></returns>
        public IMethodReturn Invoke(IMethodInvocation method, GetNextHandlerDelegate getNext)
        {
            //if caching is disabled, reset all cache and leave:
            if (!CacheConfiguration.Current.Enabled)
            {
                if (!CachesWereReset) ClearAllCaches();
                return Proceed(method, getNext);
            }

            //get the cache settings from the attribute & config:
            var cacheAttribute = GetCacheSettings(method);
            var serializer = Serializer.GetCurrent(cacheAttribute.SerializationFormat);
            var cacheKey = CacheKeyBuilder.GetCacheKey(method, serializer);
            var cache = Caching.Cache.Get(cacheAttribute.CacheType);

            //if method cache disabled, remove data and leave
            if (cacheAttribute.Disabled)
            {
                cache?.Remove(cacheKey);
                return Proceed(method, getNext);
            }

            //if there's no cache provider, leave:
            if (cache == null || cache.CacheType == CacheType.Null || serializer == null)
            {
                return Proceed(method, getNext);
            }

            var targetCategory = InstrumentCacheRequest(method);
            var returnType = ((MethodInfo)method.MethodBase).ReturnType;

            var cachedValue = cache.Get(cacheKey);

            if (cachedValue == null)
            {
                InstrumentCacheMiss(targetCategory, method);
                //call the intended method to set the return value
                var methodReturn = Proceed(method, getNext);
                //only cache if we have a real return value & no exception:
                if (methodReturn != null && methodReturn.ReturnValue != null && methodReturn.Exception == null)
                {
                    var cacheValue = serializer.Serialize(methodReturn.ReturnValue);
                    var lifespan = cacheAttribute.Lifespan;
                    if (lifespan.TotalSeconds > 0)
                    {
                        cache.Set(cacheKey, cacheValue, lifespan);
                    }
                    else
                    {
                        cache.Set(cacheKey, cacheValue);
                    }

                    SetupChangeTracking(method
                        , cacheAttribute.EntityChangeTrackingDictionary
                        , cacheKey
                        , cache.CacheType);
                }

                return methodReturn;
            }

            InstrumentCacheHit(targetCategory, method);

            var returnValue = serializer.Deserialize(returnType, cachedValue);

            return method.CreateMethodReturn(returnValue);
        }

        public void ClearAllCaches()
        {
            var allCacheTypes = Enum.GetValues(typeof(CacheType)).Cast<CacheType>();

            foreach (var cacheType in allCacheTypes)
            {
                Caching.Cache.Get(cacheType).Reset();
            }
            CachesWereReset = true;
        }

        private void InstrumentCacheHit(PerformanceCounterCategoryMetadata targetCategory, IMethodInvocation input)
        {
            if (CacheConfiguration.Current.PerformanceCounters.InstrumentCacheTotalCounts)
            {
                PerformanceCounter.IncrementCount(FxCounters.CacheTotal.CacheHits);
            }
            if (CacheConfiguration.Current.PerformanceCounters.InstrumentCacheTargetCounts)
            {
                PerformanceCounter.IncrementCount(targetCategory.CacheHits);
            }
        }

        private void InstrumentCacheMiss(PerformanceCounterCategoryMetadata targetCategory, IMethodInvocation input)
        {
            if (CacheConfiguration.Current.PerformanceCounters.InstrumentCacheTotalCounts)
            {
                PerformanceCounter.IncrementCount(FxCounters.CacheTotal.CacheMisses);
            }
            if (CacheConfiguration.Current.PerformanceCounters.InstrumentCacheTargetCounts)
            {
                PerformanceCounter.IncrementCount(targetCategory.CacheMisses);
            }
        }

        private static PerformanceCounterCategoryMetadata InstrumentCacheRequest(IMethodInvocation input)
        {
            PerformanceCounterCategoryMetadata category = null;
            if (CacheConfiguration.Current.PerformanceCounters.InstrumentCacheTotalCounts)
            {
                PerformanceCounter.IncrementCount(FxCounters.CacheTotal.CacheRequests);
            }
            if (CacheConfiguration.Current.PerformanceCounters.InstrumentCacheTargetCounts)
            {
                var cacheKey = CacheKeyBuilder.GetCacheKeyPrefix(input);
                category = new PerformanceCounterCategoryMetadata()
                {
                    Name = CacheConfiguration.Current.PerformanceCounters.CategoryNamePrefix + " - " + cacheKey,
                    Description = PerformanceCounterCategoryMetadata.DefaultDescription
                };
                PerformanceCounter.IncrementCount(category.CacheRequests);
            }
            return category;
        }

        private static CacheAttribute GetCacheSettings(IMethodInvocation input)
        {
            //get the cache attribute & check if overridden in config:
            var attributes = input.MethodBase.GetCustomAttributes(typeof(CacheAttribute), false);
            var cacheAttribute = (CacheAttribute)attributes[0];
            var cacheKeyPrefix = CacheKeyBuilder.GetCacheKeyPrefix(input);
            var targetConfig = CacheConfiguration.Current.Targets[cacheKeyPrefix];
            if (targetConfig != null)
            {
                cacheAttribute.Disabled = !targetConfig.Enabled;
                cacheAttribute.EntityCahngeTrackingToken = targetConfig.EntityChangeTracking;
                cacheAttribute.Days = targetConfig.Days;
                cacheAttribute.Hours = targetConfig.Hours;
                cacheAttribute.Minutes = targetConfig.Minutes;
                cacheAttribute.Seconds = targetConfig.Seconds;
                cacheAttribute.CacheType = targetConfig.CacheType;
                cacheAttribute.SerializationFormat = targetConfig.SerializationFormat;
            }
            if (cacheAttribute.SerializationFormat == SerializationFormat.Null)
            {
                cacheAttribute.SerializationFormat = CacheConfiguration.Current.DefaultSerializationFormat;
            }
            if (cacheAttribute.CacheType == CacheType.Null)
            {
                cacheAttribute.CacheType = CacheConfiguration.Current.DefaultCacheType;
            }
            return cacheAttribute;
        }

        private static IMethodReturn Proceed(IMethodInvocation input, GetNextHandlerDelegate getNext)
            => getNext()(input, getNext);
    }
}
