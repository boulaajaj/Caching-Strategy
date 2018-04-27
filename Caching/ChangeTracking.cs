using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Amibou.Infrastructure.Utilities;


namespace Amibou.Infrastructure.Caching
{
    public static class ChangeTrackingHandler
    {
        private const string PropertyDelimiter = ":";
        private const string ValueDelimiter = "=";

        private static ICache _trackingContainer;
        internal static ICache TrackingContainer 
            => _trackingContainer 
               ?? (_trackingContainer = Cache.Get(CacheType.Memcached));

        /// <summary>
        /// Initiates entity change tracking for the current cache key, cache type and tracking specifications
        /// </summary>
        /// <param name="method">Method being invoked</param>
        /// <param name="ChangeTrackingDictionary">Tracking specifications</param>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheType">Cache type</param>
        public static void SetupChangeTracking(IMethodInvocation method,
            Dictionary<Type, KeyValuePair<PropertyInfo, string>> ChangeTrackingDictionary,
            string cacheKey, CacheType cacheType)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
                return; //need to log this so we know this weirdness happened

            if (ChangeTrackingDictionary == null ||
                ChangeTrackingDictionary.Count == 0)
                return;

            var entityList = ChangeTrackingDictionary.Keys.ToList();

            foreach (var entity in entityList)
            {
                KeyValuePair<PropertyInfo, string> propertyParameterExpressionPair;
                var trackingToken = entity.FullName;

                ChangeTrackingDictionary.TryGetValue(entity, out propertyParameterExpressionPair);

                var property = propertyParameterExpressionPair.Key;
                var parameterExpression = propertyParameterExpressionPair.Value;

                if (property == null)
                {
                    TrackThis(trackingToken, cacheKey, cacheType); continue;
                }

                var parameterValue = ReflectionHelpers
                    .GetValueFromMethodParameterExpression(method, parameterExpression);

                if (string.IsNullOrWhiteSpace(parameterValue?.ToString()))
                {
                    TrackThis(trackingToken, cacheKey, cacheType); continue;
                }

                trackingToken += PropertyDelimiter + property.Name
                               + ValueDelimiter + parameterValue;

                TrackThis(trackingToken, cacheKey, cacheType);
            }
        }

        /// <summary>
        /// Generates a tracking token from the type, property and its value represented by
        /// the <param cref="typePropertyExpression"/>  to reset all current cache that depends
        /// on any entity record that matches these criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typePropertyExpression"></param>
        public static void RefreshCacheFor<T>(Expression<Func<T>> typePropertyExpression)
        {
            var property = ReflectionHelpers.GetMemberInfo(typePropertyExpression);
            var trackingToken = ReflectionHelpers.GetTypeFullName(typePropertyExpression);

            if (property == null)
            {
                ClearTrackedCache(trackingToken); return;
            }

            var propertyValue = ReflectionHelpers.GetMemberValue(property);

            trackingToken += PropertyDelimiter + property.Name
                           + ValueDelimiter + propertyValue;

            ClearTrackedCache(trackingToken);
        }

        /// <summary>
        /// Resets all cache that dependends on the entity type parameter
        /// </summary>
        /// <typeparam name="T">entity type</typeparam>
        public static void RefreshCacheFor<T>()
        {
            var trackingToken = typeof(T).FullName;

            ClearTrackedCache(trackingToken);
        }

        private static void TrackThis(string trackingToken, string cacheKey, CacheType cacheType)
        {

            if (!TrackingContainer.Exists(trackingToken))
            {
                TrackingContainer
                    .Set(trackingToken, new Dictionary<string, CacheType> { { cacheKey, cacheType } });
                return;
            }

            var keyTypeDictionary =
                (Dictionary<string, CacheType>)TrackingContainer.Get(trackingToken);

            CleanupInvalidOrExpiredCache(ref keyTypeDictionary);

            if (keyTypeDictionary.ContainsKey(cacheKey)) return;

            keyTypeDictionary.Add(cacheKey, cacheType);
            TrackingContainer.Set(trackingToken, keyTypeDictionary);
        }

        private static void CleanupInvalidOrExpiredCache(ref Dictionary<string, CacheType> keyTypeDictionary)
        {
            var keysToRemove = new List<string>();
            foreach (var keyTypePair in keyTypeDictionary)
            {
                var key = keyTypePair.Key;
                if(!Cache.Get(keyTypePair.Value).Exists(key))
                    keysToRemove.Add(key);
            }

            foreach (var key in keysToRemove) keyTypeDictionary.Remove(key);
        }

        private static void ClearTrackedCache(string trackingToken)
        {
            if(string.IsNullOrWhiteSpace(trackingToken)) return; //warn

            var keyTypeDictionary =
                (Dictionary<string, CacheType>)TrackingContainer
                    .Get(trackingToken);

            if (keyTypeDictionary == null) return; //warn

            foreach (var keyTypePair in keyTypeDictionary)
            {
                var cacheKey = keyTypePair.Key;
                var cacheType = keyTypePair.Value;

                Cache.Get(cacheType)?.Remove(cacheKey);
            }
        }
    }
}
