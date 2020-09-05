using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Weatherlog.Models.Parameters;

namespace Weatherlog.Computing
{
    public static class ParameterTimeSeriesExtensions
    {
        public static ParameterTimeSeries MergeSeries(this IEnumerable<ParameterTimeSeries> series)
        {
            if (series.Count() == 0)
                Trace.TraceError("Trying merge empty series list.");

            List<DateTime> targetTimes = new List<DateTime>();
            List<int> values = new List<int>();

            // TODO sometimes series.DistinctBy(s => s.GroupingTime, null) needed
            foreach (var ser in series)
            {
                targetTimes.AddRange(ser.TargetTimes);
                values.AddRange(ser.Values);
            }

            return new ParameterTimeSeries(targetTimes, values, series.First().Type, series.First().GroupingTime, series.First().GroupingTimeKind);
        }

    }
}
