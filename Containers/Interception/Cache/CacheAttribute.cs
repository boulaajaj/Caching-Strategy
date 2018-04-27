using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Reflection;
using Amibou.Infrastructure.Containers.Interception.Cache;
using Amibou.Infrastructure.Serialization;
using Amibou.Infrastructure.Utilities;

namespace Amibou.Infrastructure.Caching
{
    /// <inheritdoc />
    /// <summary>
    /// Interception attribute for methods where responses are cached
    /// </summary>
    /// <remarks>
    /// Settings applied using <see cref="T:Amibou.Infrastructure.Caching.CacheAttribute" /> can be overridden at run-time
    /// by using the <see cref="T:Amibou.Infrastructure.Configuration.CacheConfiguration" /> settings
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CacheAttribute : HandlerAttribute
    {
        /// <summary>
        /// A parsed dictionary of change tracking specifications; types plus optional property criteria
        /// </summary>
        public Dictionary<Type, KeyValuePair<PropertyInfo, string>> ChangeTrackingDictionary
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ChangeTrackingToken))
                    return null;

                var entitySpecs = ChangeTrackingToken.Split(',');
                var changeTrackingDictionary = new Dictionary<Type, KeyValuePair<PropertyInfo, string>>();
                foreach (var entitySpec in entitySpecs)
                {
                    var entityPropertyPair = entitySpec.Split(':');
                    var entityType = ReflectionHelpers.Instance
                        .GetTypeFromAppDomain(entityPropertyPair[0].Trim());

                    PropertyInfo proprety = null;
                    var parameterExpression = string.Empty;

                    if (entityPropertyPair.Length == 2)
                    {
                        var propertyParameterExpressionPair = entityPropertyPair[1].Trim().Split('=');
                        if (propertyParameterExpressionPair.Length == 2 && entityType != null)
                        {
                            proprety = entityType.GetProperty(propertyParameterExpressionPair[0]);
                            parameterExpression = propertyParameterExpressionPair[1];
                        }
                    }

                    if (entityType != null)
                        changeTrackingDictionary
                            .Add(entityType
                                , proprety != null && string.IsNullOrWhiteSpace(parameterExpression)
                                    ? new KeyValuePair<PropertyInfo, string>(proprety, parameterExpression) 
                                    : new KeyValuePair<PropertyInfo, string>()
                                );
                }

                return changeTrackingDictionary;
            }
        }

        public string ChangeTrackingToken { get; set; }

        /// <summary>
        /// Lifespan of the response in the cache
        /// </summary>
        public TimeSpan Lifespan => new TimeSpan(Days, Hours, Minutes, Seconds);

        /// <summary>
        /// Whether caching is enabled
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Days the element to be cached should live in the cache
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// Hours the element to be cached should live in the cache
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// Minutes the element to be cached should live in the cache
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Seconds the items should live in the cache
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// The type of cache required for the item
        /// </summary>
        public CacheType CacheType { get; set; }

        /// <summary>
        /// The type of serialization used for the cache key and cached item
        /// </summary>
        public SerializationFormat SerializationFormat { get; set; }


        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="T:Amibou.Infrastructure.Containers.Interception.Cache.CacheCallHandler" /> to intercept invocations
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public override ICallHandler CreateHandler(IUnityContainer container) 
            => new CacheCallHandler { Order = Order };


    }
}