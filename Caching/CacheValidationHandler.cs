using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace Amibou.Infrastructure.Caching
{
    public static class CacheValidationHandler
    {
        private const string PropertyDelimiter = ":";
        private const string ValueDelimiter = "=";

        private static ICache _validationCache;
        internal static ICache ValidationCache
        {
            get
            {
                if (_validationCache != null) return _validationCache;

                //using Memcached - it's an independent process and available
                _validationCache = Cache.Get(CacheType.Memcached);

                return _validationCache;
            }
        }

        /// <summary>
        /// extracts types from a list and/or types with a specifc proprety and parameter
        /// value from the entityChangeTrackingDictionary and then generates a unique
        /// validationToken before adding it to the cache dependency watch list; which
        /// will refresh stale cache as needed
        /// </summary>
        /// <param name="method"></param>
        /// <param name="entityChangeTrackingDictionary"></param>
        /// <param name="cacheDependencyKey"></param>
        /// <param name="cacheType"></param>
        /// <returns>'false' if current cache is valid, 'true' if cache is stale</returns>
        public static void SetupChangeTracking(IMethodInvocation method,
            Dictionary<Type, KeyValuePair<string, string>> entityChangeTrackingDictionary,
            string cacheDependencyKey,
            CacheType cacheType)
        {
            if (string.IsNullOrWhiteSpace(cacheDependencyKey))
                return; //need to log this so we know this weirdness

            if (entityChangeTrackingDictionary == null ||
                entityChangeTrackingDictionary.Count == 0)
                return;

            var entityList = entityChangeTrackingDictionary.Keys.ToList();

            foreach (var entity in entityList)
            {
                KeyValuePair<string, string> propertyParameterPair;
                var validationToken = $"{entity.FullName}";

                entityChangeTrackingDictionary.TryGetValue(entity, out propertyParameterPair);

                if (propertyParameterPair.Key == null)
                {
                    AddToCacheDependencyList(validationToken, cacheDependencyKey, cacheType);
                    continue;
                }

                var propertyName = propertyParameterPair.Key;
                var parameterExpression = propertyParameterPair.Value.Split('.');
                var inputIndex = method.Inputs.GetParameterInfo(parameterExpression[0]).Position;
                var methodInputTargetValue = method.Inputs[inputIndex];

                methodInputTargetValue = GetValueFromExpressionArray(parameterExpression, methodInputTargetValue);

                if (methodInputTargetValue == null 
                    || string.IsNullOrWhiteSpace(methodInputTargetValue.ToString()))
                {
                    AddToCacheDependencyList(validationToken, cacheDependencyKey, cacheType);
                    continue;
                }

                var typeProperty = entity.GetProperty(propertyName);

                if (typeProperty == null)
                {
                    AddToCacheDependencyList(validationToken, cacheDependencyKey, cacheType);
                    continue;
                }

                validationToken +=
                        $"{PropertyDelimiter}{typeProperty.Name}" +
                        $"{ValueDelimiter}{methodInputTargetValue}";

                AddToCacheDependencyList(validationToken, cacheDependencyKey, cacheType);
            }
        }

        /// <summary>
        /// From a lamda expression, this exposes the type, property and its value,
        /// with which a validation token is generated; which is used to retreive all the
        /// stale cache keys before resetting/removing them
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typePropertyExpression"></param>
        public static void RefreshStaleCacheFor<T>(Expression<Func<T>> typePropertyExpression)
        {
            var property = GetMemberInfo(typePropertyExpression);
            var typePath = GetTypePath(typePropertyExpression);
            var validationToken = typePath;

            if (property == null)
            {
                ResetDependentCaches(validationToken);
                return;
            }

            var propertyReflectedType = property.Member.ReflectedType;
            if (propertyReflectedType == null)
                throw new Exception("UTS.Caching RefreshStaleCacheFor - Invalid property");

            var propertyValue = propertyReflectedType.TypeHandle.Value;

            validationToken += $"{PropertyDelimiter}{property.Member.Name}" +
                               $"{ValueDelimiter}{propertyValue}";

            ResetDependentCaches(validationToken);
        }

        /// <summary>
        /// Resets all dependent cache keys rely on the type being passed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RefreshStaleCacheFor<T>()
        {
            var validationToken = typeof(T).FullName;

            ResetDependentCaches(validationToken);
        }

        private static object GetValueFromExpressionArray(IReadOnlyList<string> parameterExpression,
            object methodInputTargetValue)
        {
            if (parameterExpression.Count <= 1) return methodInputTargetValue;

            for (var i = 1; i < parameterExpression.Count; i++)
            {
                if (methodInputTargetValue == null) return null;

                methodInputTargetValue = methodInputTargetValue
                    .GetType()
                    .GetProperty(parameterExpression[i])
                    ?.GetValue(methodInputTargetValue);
            }

            return methodInputTargetValue;
        }

        private static void AddToCacheDependencyList(string validationToken,
                    string cacheDependencyKey,
                    CacheType cacheType)
        {
            var dependenciesToken = $"{validationToken}";

            if (!ValidationCache.Exists(dependenciesToken))
            {
                var cacheDependencyDictionary =
                    new Dictionary<string, CacheType> { { cacheDependencyKey, cacheType } };
                ValidationCache.Set(dependenciesToken, cacheDependencyDictionary);
                return;
            }

            var currentDependencyDictionary =
                (Dictionary<string, CacheType>)ValidationCache.Get(dependenciesToken);
            if (currentDependencyDictionary.ContainsKey(cacheDependencyKey)) return;

            currentDependencyDictionary.Add(cacheDependencyKey, cacheType);
            ValidationCache.Set(dependenciesToken, currentDependencyDictionary);
        }

        private static void ResetDependentCaches(string validationToken)
        {
            var dependencyCacheList =
                (Dictionary<string, CacheType>)ValidationCache
                    .Get($"{validationToken}");

            foreach (var keyCacheTypePair in dependencyCacheList)
            {
                Cache.Get(keyCacheTypePair.Value)?.Remove(keyCacheTypePair.Key);
            }
        }
        private static string GetTypePath<T>(Expression<Func<T>> property)
        {
            var expression = GetMemberInfo(property);
            return expression.Member.DeclaringType != null
                ? expression.Member.DeclaringType.FullName
                : null;
        }

        private static MemberExpression GetMemberInfo(Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException(nameof(method));

            MemberExpression memberExpr = null;

            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Convert:
                    memberExpr =
                        ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                    break;
                case ExpressionType.MemberAccess:
                    memberExpr = lambda.Body as MemberExpression;
                    break;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }

    }
}
