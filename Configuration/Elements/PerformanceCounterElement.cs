
using System.Configuration;
using Amibou.Infrastructure;
using Amibou.Infrastructure.Serialization;

namespace Amibou.Infrastructure.Configuration
{
    public class PerformanceCounterElement : ConfigurationElement
    {
        [ConfigurationProperty(SettingName.InstrumentCacheTotalCounts)]
        public bool InstrumentCacheTotalCounts => (bool)this[SettingName.InstrumentCacheTotalCounts];

        [ConfigurationProperty(SettingName.InstrumentCacheTargetCounts)]
        public bool InstrumentCacheTargetCounts => (bool)this[SettingName.InstrumentCacheTargetCounts];

        [ConfigurationProperty(SettingName.CategoryNamePrefix, DefaultValue = "UTS Cache")]
        public string CategoryNamePrefix => (string)this[SettingName.CategoryNamePrefix];

        /// <summary>
        /// Constants for indexing settings
        /// </summary>
        private struct SettingName
        {
            /// <summary>
            /// instrumentCacheTotalCounts
            /// </summary>
            public const string InstrumentCacheTotalCounts = "instrumentCacheTotalCounts";

            /// <summary>
            /// instrumentCacheTargetCounts
            /// </summary>
            public const string InstrumentCacheTargetCounts = "instrumentCacheTargetCounts";

            public const string CategoryNamePrefix = "categoryNamePrefix";
        }
    }
}

