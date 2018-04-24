using Amibou.Infrastructure.Configuration;
using Amibou.Infrastructure.Extensions;
using Amibou.Infrastructure.Instrumentation.PerformanceCounters;
using Amibou.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using diag = System.Diagnostics;

namespace Amibou.Infrastructure.Instrumentation
{
    public static class PerformanceCounter
    {
        private static Dictionary<string, diag.PerformanceCounter> _Counters = new Dictionary<string, diag.PerformanceCounter>();

        private static void EnsureCounter(PerformanceCounterMetadata counter)
        {
            if (!_Counters.ContainsKey(counter.FullName))
            {
                try
                {
                    if (!PerformanceCounterCategory.Exists(counter.Category.Name))
                    {
                        CreateCounters(counter.Category);
                    }
                    _Counters[counter.FullName] = new diag.PerformanceCounter(counter.Category.Name, counter.Name, false);
                }
                catch (Exception ex)
                {
                    //Log.Warn("Instrumentation.EnsureCounters failed to initialise performance counter: {0}. Counter may not be initialised. Error: {1}", counter.Name, ex.FullMessage());
                }
            }
        }

        public static void IncrementCount(PerformanceCounterMetadata counter)
        {
            EnsureCounter(counter);
            if (counter.Type == PerformanceCounterType.NumberOfItems64)
            {
                if (_Counters.ContainsKey(counter.FullName))
                {
                    try
                    {
                        _Counters[counter.FullName].Increment();
                    }
                    catch (Exception ex)
                    {
                        //Log.Debug("Instrumentation.IncrementCounter failed for: {0}. Error: {1}", counter.FullName, ex.FullMessage());
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the custom counters
        /// </summary>
        public static void CreateCounters(PerformanceCounterCategoryMetadata category)
        {
            try
            {
                if (!PerformanceCounterCategory.Exists(category.Name))
                {
                    CounterCreationDataCollection counterCollection = new CounterCreationDataCollection();
                    foreach (var description in category.Counters)
                    {
                        var counter = new CounterCreationData();
                        counter.CounterName = description.Name;
                        counter.CounterHelp = description.Description;
                        counter.CounterType = description.Type;
                        counterCollection.Add(counter);
                    }
                    PerformanceCounterCategory.Create(category.Name, category.Description, PerformanceCounterCategoryType.SingleInstance, counterCollection);
                    //Log.Debug("{0} counter category Created", category.Name);
                }
            }
            catch (Exception ex)
            {
                //Log.Warn("Instrumentation.CreateCounters failed, performance counters may not be initialised. Error: {0}", ex.FullMessage());
            }
        }
    }
}