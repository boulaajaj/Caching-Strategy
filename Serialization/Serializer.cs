using Amibou.Infrastructure.Containers;
using System.Linq;

namespace Amibou.Infrastructure.Serialization
{
    public static class Serializer
    {
        public static ISerializer GetCurrent(SerializationFormat format)
        {            
            var serializers = Container.GetAll<ISerializer>();
            return (from s in serializers
                    where s.Format == format
                    select s).FirstOrDefault();
        }

        public static ISerializer Json => GetCurrent(SerializationFormat.Json);

        public static ISerializer Xml => GetCurrent(SerializationFormat.Xml);

        public static ISerializer Binary => GetCurrent(SerializationFormat.Binary);
    }
}
