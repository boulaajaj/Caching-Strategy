﻿using System.Diagnostics;

namespace Amibou.Infrastructure.Instrumentation
{
    public class PerformanceCounterMetadata
    {
        public PerformanceCounterCategoryMetadata Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PerformanceCounterType Type { get; set; }

        public string FullName => $"{Category.Name}_{Name}";
    }
}