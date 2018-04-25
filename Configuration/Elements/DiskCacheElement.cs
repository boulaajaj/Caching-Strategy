using System.Configuration;

namespace Amibou.Infrastructure.Configuration
{
    public class DiskCacheElement : ConfigurationElement
    {
        [ConfigurationProperty(SettingName.Path, DefaultValue = "LocalDiskCache.cache")]
        public string Path => (string)this[SettingName.Path];

        [ConfigurationProperty(SettingName.EncryptItems, DefaultValue = true)]
        public bool EncryptItems => (bool)this[SettingName.EncryptItems];

        [ConfigurationProperty(SettingName.MaxSizeInMb, DefaultValue = 200)]
        public int MaxSizeInMb => (int)this[SettingName.MaxSizeInMb];

        /// <summary>
        /// Constants for indexing settings
        /// </summary>
        private struct SettingName
        {
            /// <summary>
            /// path
            /// </summary>
            public const string Path = "path";

            /// <summary>
            /// encryptItems
            /// </summary>
            public const string EncryptItems = "encryptItems";

            /// <summary>
            /// maxSize
            /// </summary>
            public const string MaxSizeInMb = "maxSizeInMb";
        }
    }
}
