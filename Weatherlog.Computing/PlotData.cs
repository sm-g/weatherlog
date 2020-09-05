using System.Collections.Generic;
using System.Linq;
using Weatherlog.Models.Sources;

namespace Weatherlog.Computing
{
    public class PlotData
    {
        public readonly int PlotGroupKey;
        public readonly string ParameterType;
        public readonly Dictionary<IAbstractDataSource, IEnumerable<ParameterTimeSeries>> Values;

        /// <summary>
        /// Plot with many series for source.
        /// </summary>
        public PlotData(int plotGroup, string parameterType, Dictionary<IAbstractDataSource, IEnumerable<ParameterTimeSeries>> dict)
        {
            PlotGroupKey = plotGroup;
            Values = new Dictionary<IAbstractDataSource, IEnumerable<ParameterTimeSeries>>(dict.Count);
            foreach (var item in dict)
            {
                if (item.Value.Count() == 0)
                    continue;

                if (item.Key is RealDataSource)
                {
                    Values.Add(item.Key, item.Value.MergeSeries().ToIEnumerable());
                }
                else
                {
                    Values.Add(item.Key, item.Value);
                }
            }
            ParameterType = parameterType;
        }

        /// <summary>
        /// Plot with one series for source.
        /// </summary>
        public PlotData(int plotGroup, string parameterType, Dictionary<IAbstractDataSource, ParameterTimeSeries> dict)
        {
            PlotGroupKey = plotGroup;
            Values = new Dictionary<IAbstractDataSource, IEnumerable<ParameterTimeSeries>>(dict.Count);
            foreach (var item in dict)
            {
                Values.Add(item.Key, item.Value.ToIEnumerable());
            }
            ParameterType = parameterType;
        }
    }
}
