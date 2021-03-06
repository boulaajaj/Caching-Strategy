﻿using Amibou.Infrastructure.Configuration;

namespace Amibou.Infrastructure.Instrumentation.PerformanceCounters
{
    public static class FxCounters
    {
        public static PerformanceCounterCategoryMetadata CacheTotal { get; private set; }

        static FxCounters()
        {
            CacheTotal = new PerformanceCounterCategoryMetadata()
            {
                Name = CacheConfiguration.Current.PerformanceCounters.CategoryNamePrefix + " - Totals",
                Description = PerformanceCounterCategoryMetadata.DefaultDescription
            };
        }
    }
}