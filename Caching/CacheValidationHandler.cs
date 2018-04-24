using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


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
                
                //using Memcached - it is an independent process and easily available
                _validationCache = Cache.Get(CacheType.Memcached); 

                return _validationCache;
            }
        }

        /// <summary>
        /// Checks if a list of types or a list of types with a specifc proprety have been marked as stale
        /// </summary>
        /// <param name="method"></param>
        /// <param name="entityChangeTrackingDictionary"></param>
        /// <returns>'false' if current cache is valid, 'true' if cache is stale</returns>
        public static bool IsStale(IMethodInvocation method,
            Dictionary<Type, KeyValuePair<string, string>> entityChangeTrackingDictionary)
        {
            if (entityChangeTrackingDictionary == null || entityChangeTrackingDictionary.Count == 0)
                return false;

            var entityList = entityChangeTrackingDictionary.Keys.ToList();

            foreach (var entity in entityList)
            {
                KeyValuePair<string, string> propertyParameterPair;
                var validationToken = $"{entity.FullName}";

                if (!entityChangeTrackingDictionary.TryGetValue(entity, out propertyParameterPair))
                    return ValidationCache.Exists(validationToken);

                if (propertyParameterPair.Key == null) return ValidationCache.Exists(validationToken);

                var propertyName = propertyParameterPair.Key;
                var parameterExpression = propertyParameterPair.Value.Split('.');
                var inputIndex = method.Inputs.GetParameterInfo(parameterExpression[0]).Position;
                var methodInputTargetValue = method.Inputs[inputIndex];

                if (parameterExpression.Length > 1)
                {
                    for (var i = 1; i < parameterExpression.Length; i++)
                    {
                        if (methodInputTargetValue == null) break;

                        methodInputTargetValue = methodInputTargetValue
                                .GetType()
                                .GetProperty(parameterExpression[i])
                                ?.GetValue(methodInputTargetValue);
                    }
                }
                var typeProperty = entity.GetProperty(propertyName);

                if (methodInputTargetValue == null || 
                    string.IsNullOrWhiteSpace(methodInputTargetValue.ToString()) ||
                    typeProperty == null) 
                    return ValidationCache.Exists(validationToken);

                validationToken +=
                        $"{PropertyDelimiter}{typeProperty.Name}" +
                        $"{ValueDelimiter}{methodInputTargetValue}";

                return ValidationCache.Exists(validationToken);
            }
            return false;
        }

        /// <summary>
        /// From a lamda expression, this exposes the type, property and its value, then
        /// inserts a concatinated validation token to mark all related cache items for refresh
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        public static void SetStaleCacheValidationToken<T>(Expression<Func<T>> propertyExpression)
        {
            var typePath = GetTypePath(propertyExpression);
            var property = GetMemberInfo(propertyExpression);
            var validationToken = typePath;

            if (property == null)
            {
                if(ValidationCache.Exists(validationToken)) return;
                ValidationCache.Set(validationToken, true);
                return;
            }

            var propertyReflectedType = property.Member.ReflectedType;
            if (propertyReflectedType == null)
                throw  new Exception("UTS.Caching SetCacheValidationToken - Invalid property");
            
            var propertyValue = propertyReflectedType.TypeHandle.Value;

            validationToken += $"{PropertyDelimiter}{property.Member.Name}" +
                               $"{ValueDelimiter}{propertyValue}";

            if (ValidationCache.Exists(validationToken)) return;
            ValidationCache.Set(validationToken, true);
        }
        /// <summary>
        /// Inserts the type's fullname as a validation token to mark all related cache items for refresh
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetStaleCacheValidationToken<T>()
        {
            var validationToken = typeof(T).FullName;

            if (ValidationCache.Exists(validationToken)) return;
            ValidationCache.Set(validationToken, true);
        }

        public static string GetTypePath<T>(Expression<Func<T>> property)
        {
            var expression = GetMemberInfo(property);
            if (expression.Member.DeclaringType != null)
            {
                return expression.Member.DeclaringType.FullName;
            }

            return null;
        }

        private static MemberExpression GetMemberInfo(Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException(nameof(method));

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }

    }
}
