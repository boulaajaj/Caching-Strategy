using Amibou.Infrastructure.Containers.Interception.Cache;
using Amibou.Infrastructure.Serialization;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using static Amibou.Infrastructure.Utilities.Utilities;

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
        /// A list of entity types associated with a string that represents the name of the unique identifier of the
        /// entity record; if the string is null,, the cache will become invalid everytime a record is deleted or added 
        /// </summary>
        public Dictionary<Type, KeyValuePair<string, string>> EntityChangeTrackingDictionary
        {
            get
            {
                if (string.IsNullOrWhiteSpace(EntityCahngeTrackingToken))
                    return null;

                var entitySpecs = EntityCahngeTrackingToken.Split(',');
                var entityChangeTrackingDictionary = new Dictionary<Type, KeyValuePair<string, string>>();
                foreach (var entitySpec in entitySpecs)
                {
                    var entityProperty = entitySpec.Split(':');
                    var entityName = Instance.GetTypeFromAppDomain(entityProperty[0].Trim());
                    var entityPropertyArray = entityProperty.Length > 1 ? entityProperty[1].Trim().Split('=') : null;

                    if (entityName != null)
                        entityChangeTrackingDictionary
                            .Add(entityName
                                , entityPropertyArray != null 
                                    ? new KeyValuePair<string, string>(entityPropertyArray[0], entityPropertyArray[1]) 
                                    : new KeyValuePair<string, string>(null, null));
                }

                return entityChangeTrackingDictionary;
            }
        }

        public string EntityCahngeTrackingToken { get; set; }

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