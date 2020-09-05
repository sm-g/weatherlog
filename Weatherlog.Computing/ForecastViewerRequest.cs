using System;
using System.Collections.Generic;
using Weatherlog.Models;
using Weatherlog.Models.Parameters;
using Weatherlog.Models.Sources;

namespace Weatherlog.Computing
{
    public enum RequestTimeDeterminateness
    {
        AllDay,
        DateAndHour
    }

    public class ForecastViewerRequest
    {
        public readonly Station Station;
        public readonly DateTime DateTime;
        public readonly GivenForecastTimeKind GivenTime;
        public readonly RequestTimeDeterminateness TimeDeterminateness;
        public readonly IEnumerable<IAbstractDataSource> Sources;
        public readonly IEnumerable<string> ParameterTypes;
        public ForecastViewerRequest(Station station, DateTime dateOrDateTime, GivenForecastTimeKind givenMode, RequestTimeDeterminateness dateTimeMode, IEnumerable<IAbstractDataSource> sources = null, IEnumerable<string> parameterTypes = null)
        {
            Station = station;
            DateTime = dateOrDateTime;
            GivenTime = givenMode;
            TimeDeterminateness = dateTimeMode;
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
