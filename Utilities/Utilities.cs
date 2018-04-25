using Amibou.Infrastructure.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Amibou.Infrastructure.Utilities
{
    public class Utilities
    {
        public static Utilities Instance => Containers.Container.Get<Utilities>();

        [Cache(CacheType = CacheType.Memory)]
        public virtual Type GetTypeFromAppDomain(string typeName)
        {
            string pattern;

            if (typeName.Contains('.'))
                pattern = @".*" + typeName + "$";
            else
                pattern = @".*\." + typeName + "$";

            var regex = new Regex(pattern);
            var typeList = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                typeList
                    .AddRange(assembly
                        .GetTypes()
                        .Where(
                            t => t.FullName != null && regex.IsMatch(t.FullName)
                        )
                        .ToList());

                if (typeList.Count == 0) continue;

                if (typeList.Count == 1) return typeList.First();

                if (typeList.Count > 1)
                    throw new Exception(
                        "UTS.Caching Entity Change Tracking - Ambiguous type Name for current AppDomain. Please provide a Fully Qualified Type Name instead.");
            }

            throw new Exception(
                "UTS.Caching Entity Change Tracking - Invalid Type Name for current AppDomain.");
        }
    }
}