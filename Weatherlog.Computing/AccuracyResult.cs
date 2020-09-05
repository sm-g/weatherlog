using System.Collections.Generic;
using Weatherlog.Models.Sources;

namespace Weatherlog.Computing
{
    public class AccuracyResult
    {
        public readonly string ParameterType;
        public readonly RealDataSource RealSource;
        public readonly Dictionary<ForecastDataSource, Dictionary<StatisticMethods, double>> Values;

        public AccuracyResult(string parameterType, RealDataSource realSource, Dictionary<ForecastDataSource, Dictionary<StatisticMethods, double>> dict)
        {
            Values = dict;
            RealSource = realSource;
            ParameterType = parameterType;
        }
    }
}
