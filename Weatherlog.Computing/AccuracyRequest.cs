using System;
using System.Collections.Generic;
using Weatherlog.Models;
using Weatherlog.Models.Parameters;
using Weatherlog.Models.Sources;

namespace Weatherlog.Computing
{
    public enum AccuracyComparison
    {
        HalfDay,
        OneDay,
        FiveDaysByOneDay
    }

    public class AccuracyRequest
    {
        public readonly Station Station;
        public readonly DateTime Date;
        public readonly AccuracyComparison AccuracyComparison;
        public readonly IEnumerable<IAbstractDataSource> Sources;
        public readonly IEnumerable<string> ParameterTypes;
        public AccuracyRequest(Station station, DateTime date, AccuracyComparison comparisonMode, IEnumerable<IAbstractDataSource> sources = null, IEnumerable<string> parameterTypes = null)
        {
            Station = station;
            Date = date;
            AccuracyComparison = comparisonMode;
            if (sources != null)
                Sources = sources;
            else
                Sources = SourcesDirector.Instance.ForecastDataSources;

            if (parameterTypes != null)
                ParameterTypes = parameterTypes;
            else
                ParameterTypes = ParametersFactory.KnownParameterTypesNames;
        }
    }
}
