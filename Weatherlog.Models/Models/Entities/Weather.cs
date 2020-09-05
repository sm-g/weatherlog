using System;
using System.Collections.Generic;
using Weatherlog.Models.Sources;

namespace Weatherlog.Models
{
    public class Weather
    {
        public Station Station { get; private set; }
        public IAbstractDataSource Source { get; private set; }
        public DateTime FetchedTime { get; private set; }
        public IEnumerable<Forecast> Forecasts { get; private set; }

        public Weather(Station station, IAbstractDataSource source, IEnumerable<Forecast> forecasts)
        {
            Station = station;
            Source = source;
            Forecasts = forecasts;
            FetchedTime = DateTime.Now;
        }
    }
}
