using System;
using System.Collections.Generic;
using System.Linq;
using Weatherlog.Models.Parameters;

namespace Weatherlog.Computing
{
    public class ParameterTimeSeries
    {
        private SortedList<DateTime, int> _timeValuePairs;
        private List<double> _realValues;

        public string Type { get; private set; }

        public GivenForecastTimeKind GroupingTimeKind { get; private set; }

        public DateTime GroupingTime { get; private set; }

        public IList<DateTime> TargetTimes
        {
            get { return _timeValuePairs.Keys; }
        }

        public IList<int> Values
        {
            get { return _timeValuePairs.Values; }
        }

        public IList<double> RealValues
        {
            get
            {
                if (_realValues == null)
                {
                    if (ParametersFactory.IsSpecialCaseValue(Type))
                        _realValues = Values.Select(v => ParametersFactory.GetRealDouble(Type, v)).ToList();
                    else
                        _realValues = Values.Select(v => (double)v).ToList();
                }
                return _realValues;
            }
        }

        public int Length
        {
            get { return _timeValuePairs.Count; }
        }

        public DateTime MinTargetTime
        {
            get { return TargetTimes.First(); }
        }

        public DateTime MaxTargetTime
        {
            get { return TargetTimes.Last(); }
        }

        public int MaxValue
        {
            get { return Values.Max(); }
        }
        public int MinValue
        {
            get { return Values.Min(); }
        }
        public ParameterTimeSeries(List<DateTime> targetTimes, List<int> values, string type, DateTime groupingTime, GivenForecastTimeKind groupingTimeKind)
        {
            if (targetTimes.Count != values.Count)
            {
                throw new ArgumentException("The size of the lists do not match.");
            }

            Type = type;
            var count = targetTimes.Count;
            _timeValuePairs = new SortedList<DateTime, int>(count);

            for (int i = 0; i < count; i++)
            {
                _timeValuePairs.Add(targetTimes[i], values[i]);
            }
            GroupingTime = groupingTime;
            GroupingTimeKind = groupingTimeKind;
        }

        public ParameterTimeSeries(Dictionary<DateTime, int> targetTimeValuesDict, string type, DateTime groupingTime, GivenForecastTimeKind groupingTimeKind)
        {
            Type = type;
            _timeValuePairs = new SortedList<DateTime, int>(targetTimeValuesDict);
            GroupingTime = groupingTime;
            GroupingTimeKind = groupingTimeKind;
        }
    }
}
