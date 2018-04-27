using System.Configuration;
using Amibou.Infrastructure.Caching;
using Amibou.Infrastructure.Serialization;

namespace Amibou.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration section for configuring caching
    /// </summary>
    public class CacheConfiguration : ConfigurationSection
    {
        private static bool _loggedWarning;

        /// <summary>
        /// Returns the currently configured settings
        /// </summary>
        public static CacheConfiguration Current
        {
            get
            {
                var current = ConfigurationManager.GetSection("Amibou.CacheConfiguration") as CacheConfiguration;
                if (current == null)
                {
                    current = new CacheConfiguration();
                    if (!_loggedWarning)
                    {
                        //Log.Warn("Configuration section: <Amibou.CacheConfiguration> not specified. Default configuration will be used");
                        _loggedWarning = true;
                    }
                }
                return current; 
            }
        }

        /// <summary>
        /// Returns the root path configuration settings
        /// </summary>
        [ConfigurationProperty(SettingName.Enabled, DefaultValue = true)]
        public bool Enabled => (bool)this[SettingName.Enabled];

        /// <summary>
        /// Returns the configuration settins for cache targets
        /// </summary>
        [ConfigurationProperty(SettingName.Targets)]
        public CacheTargetCollection Targets => (CacheTargetCollection)this[SettingName.Targets];

        /// <summary>
        /// Returns the root path configuration settings
        /// </summary>
        [ConfigurationProperty(SettingName.HashPrefixInCacheKey, DefaultValue = false)]
        public bool HashPrefixInCacheKey => (bool)this[SettingName.HashPrefixInCacheKey];

        /// <summary>
        /// Returns the configured name of the out-of-process cache
        /// </summary>
        [ConfigurationProperty(SettingName.DefaultCacheName, DefaultValue = "AmibouCache")]
        public string DefaultCacheName => (string)this[SettingName.DefaultCacheName];

        [ConfigurationProperty(SettingName.DefaultCacheType, DefaultValue = CacheType.Memory)]
        public CacheType DefaultCacheType => (CacheType)this[SettingName.DefaultCacheType];

        [ConfigurationProperty(SettingName.DefaultSerializationFormat, DefaultValue = SerializationFormat.Json)]
        public SerializationFormat DefaultSerializationFormat => (SerializationFormat)this[SettingName.DefaultSerializationFormat];

        [ConfigurationProperty(SettingName.DiskCache)]
        public DiskCacheElement DiskCache => (DiskCacheElement)this[SettingName.DiskCache];

        [ConfigurationProperty(SettingName.PerformanceCounters)]
        public PerformanceCounterElement PerformanceCounters => (PerformanceCounterElement)this[SettingName.PerformanceCounters];

        /// <summary>
        /// Constants for indexing settings
        /// </summary>
        private struct SettingName
        {
            /// <summary>
            /// enabled
            /// </summary>
            public const string Enabled = "enabled";

            /// <summary>
            /// targets
            /// </summary>
            public const string Targets = "targets";

            /// <summary>
            /// defaultCacheName
            /// </summary>
            public const string DefaultCacheName = "defaultCacheName";

            /// <summary>
            /// defaultCacheType
            /// </summary>
            public const string DefaultCacheType = "defaultCacheType";

            /// <summary>
            /// defaultSerializationFormat
            /// </summary>
            public const string DefaultSerializationFormat = "defaultSerializationFormat";

            /// <summary>
            /// hashPrefixInCacheKey
            /// </summary>
            public const string HashPrefixInCacheKey = "hashPrefixInCacheKey";

            /// <summary>
            /// diskCache
            /// </summary>
            public const string DiskCache = "diskCache";

            /// <summary>
            /// performanceCounters
            /// </summary>
            public const string PerformanceCounters = "performanceCounters";
        }
    }
}
