using Amibou.Infrastructure.Serialization;

namespace Amibou.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions to <see cref="ISerializer"/>
    /// </summary>
    public static class ISerializerExtensions
    {
        public static T Deserialize<T>(this ISerializer serializer, object serializedValue) where T : class 
            => serializer.Deserialize(typeof(T), serializedValue) as T;
    }
}
