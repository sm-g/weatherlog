using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Weatherlog.Models.Sources;

namespace Weatherlog.Computing
{
    public class AccuracyMeter
    {
        public static AccuracyResult CalcAccuracy(PlotData data, StatisticMethods method)
        {
            return CalcAccuracy(data, method.ToIEnumerable());
        }
        public static AccuracyResult CalcAccuracy(PlotData data, IEnumerable<StatisticMethods> methods)
        {
            var realSource = (RealDataSource)data.Values.Keys.Where(s => s is RealDataSource).First();
            var answers = new Dictionary<ForecastDataSource, Dictionary<StatisticMethods, double>>();

            var result = new AccuracyResult(data.ParameterType, realSource, answers);

            var realSourceSeries = data.Values[realSource].First();

            var forecastSources = data.Values.Keys.Where(s => s is ForecastDataSource).Cast<ForecastDataSource>();

            foreach (var source in forecastSources.DistinctBy(s => s.Id, null))
            {
                foreach (var series in data.Values[source])
                {
                    var filteredRealSeries = FitTimes(realSourceSeries, series);
                    answers.Add(source, Statistic.CalculateMany(methods, series, filteredRealSeries));
                }
            }

            return result;
        }

        private static ParameterTimeSeries FitTimes(ParameterTimeSeries large, ParameterTimeSeries narrow)
        {
            if (large.Type != narrow.Type)
                return large;

            List<DateTime> targetTimes = new List<DateTime>();
            List<int> values = new List<int>();

            if (narrow.Length > 1)
            {
                int iLarge = 0;
                int iNarrow = 0;
                while (iLarge < large.Length && iNarrow < narrow.Length)
                {
                    if (large.TargetTimes[iLarge] >= narrow.TargetTimes[iNarrow])
                    {
                        targetTimes.Add(large.TargetTimes[iLarge]);
                        values.Add(large.Values[iLarge]);
                        iNarrow++;
                    }
                    iLarge++;
                }
            }
            else if (narrow.Length == 1)
            {
                targetTimes.Add(narrow.MinTargetTime);
                values.Add(large.MaxValue);
            }

            return new ParameterTimeSeries(targetTimes, values, large.Type, large.GroupingTime, large.GroupingTimeKind);
        }

        static AccuracyMeter()
        {
            Statistic.CalculatingFailed += Statistic_CalculatingFailed;
        }

        static void Statistic_CalculatingFailed(object sender, StatisticCalculatingFailedEventArgs e)
        {
            Trace.TraceError("Statistic {0} {1}.", e.method, e.message);
        }
    }
}
