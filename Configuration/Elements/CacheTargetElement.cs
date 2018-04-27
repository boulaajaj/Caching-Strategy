using System.Configuration;
using Amibou.Infrastructure.Caching;
using Amibou.Infrastructure.Serialization;

namespace Amibou.Infrastructure.Configuration
{
    /// <inheritdoc />
    /// <summary>
    /// Element for configuring cache targets
    /// </summary>
    public class CacheTargetElement : ConfigurationElement
    {
        /// <summary>
        /// Returns the key for the cache target
        /// </summary>
        [ConfigurationProperty(SettingName.KeyPrefix)]
        public string KeyPrefix => (string) this[SettingName.KeyPrefix];

        /// <summary>
        /// Returns change tracking specifications; list of entity types and/or
        /// entity record criteria
        /// </summary>
        [ConfigurationProperty(SettingName.ChangeTrackingToken)]
        public string ChangeTrackingToken => (string)this[SettingName.ChangeTrackingToken];

        /// <summary>
        /// Returns whether caching is enabled for the target
        /// </summary>
        [ConfigurationProperty(SettingName.Enabled, DefaultValue = true)]
        public bool Enabled => (bool)this[SettingName.Enabled];

        /// <summary>
        /// Gets the number of Days to cache the target value
        /// </summary>
        [ConfigurationProperty(SettingName.Days, DefaultValue = 0)]
        public int Days => (int)this[SettingName.Days];

        /// <summary>
        /// Gets the number of Hours to cache the target value
        /// </summary>
        [ConfigurationProperty(SettingName.Hours, DefaultValue = 0)]
        public int Hours => (int)this[SettingName.Hours];

        /// <summary>
        /// Gets the number of Minutes to cache the target value
        /// </summary>
        [ConfigurationProperty(SettingName.Minutes, DefaultValue = 0)]
        public int Minutes => (int)this[SettingName.Minutes];

        /// <summary>
        /// Gets the number of Seconds to cache the target value
        /// </summary>
        [ConfigurationProperty(SettingName.Seconds, DefaultValue = 0)]
        public int Seconds => (int)this[SettingName.Seconds];

        /// <summary>
        /// Gets the type of cache for the target value
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="CacheType.InProcess"/>
        /// </remarks>
        [ConfigurationProperty(SettingName.CacheType, DefaultValue = CacheType.Memory)]
        public CacheType CacheType => (CacheType)this[SettingName.CacheType];

        /// <summary>
        /// The type of serialization used for the cache key and cached item
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="SerializationFormat.Json"/>
        /// </remarks>
        [ConfigurationProperty(SettingName.CacheType, DefaultValue = SerializationFormat.Json)]
        public SerializationFormat SerializationFormat { get; set; }

        /// <summary>
        /// Constants for indexing settings
        /// </summary>
        private struct SettingName
        {
            /// <summary>
            /// keyPrefix
            /// </summary>
            public const string KeyPrefix = "keyPrefix";

            /// <summary>
            /// validationToken
            /// </summary>
            public const string ChangeTrackingToken = "changeTrackingToken";

            /// <summary>
            /// enabled
            /// </summary>
            public const string Enabled = "enabled";
            
            
            /// <summary>
            /// days
            /// </summary>
            public const string Days = "days";
            
            /// <summary>
            /// hours
            /// </summary>
            public const string Hours = "hours";
            
            /// <summary>
            /// minutes
            /// </summary>
            public const string Minutes = "minutes";
            
            /// <summary>
            /// seconds
            /// </summary>
            public const string Seconds = "seconds";

            /// <summary>
            /// cacheType
            /// </summary>
            public const string CacheType = "cacheType";
        }
    }
}

