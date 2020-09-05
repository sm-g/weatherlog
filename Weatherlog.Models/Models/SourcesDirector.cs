using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Weatherlog.Data;

namespace Weatherlog.Models.Sources
{
    public class SourcesDirector
    {
        private readonly List<ForecastDataSource> _forecastSources = new List<ForecastDataSource>();
        private readonly List<RealDataSource> _realSources = new List<RealDataSource>();
        private ForecastDataSource _dummySource;
        private readonly ConcurrentDictionary<string, Dictionary<string, DateTime>> _lastFetchTimes = new ConcurrentDictionary<string, Dictionary<string, DateTime>>();

        public IEnumerable<ForecastDataSource> ForecastDataSources
        {
            get
            {
                return _forecastSources;
            }
        }
        public IEnumerable<RealDataSource> RealDataSources
        {
            get
            {
                return _realSources;
            }
        }
        /// <summary>
        /// Gets all data sources, forecast data sources come first.
        /// </summary>
        public IEnumerable<IAbstractDataSource> AllDataSources
        {
            get
            {
                return ForecastDataSources.Union<IAbstractDataSource>(RealDataSources);
            }
        }

        public IAbstractDataSource Dummy
        {
            get
            {
                if (_dummySource == null)
                    _dummySource = new DummySource();
                return _dummySource;
            }
        }

        public IAbstractDataSource GetSourceById(string id)
        {
            var source = AllDataSources.FirstOrDefault<IAbstractDataSource>(x => x.Id == id);
            if (source == null)
                return Dummy;
            else
                return source;
        }

        public DateTime GetStationLastFetchTime(IAbstractDataSource source, Station station)
        {
            Dictionary<string, DateTime> sourceTimes;
            if (!_lastFetchTimes.TryGetValue(station.Id, out sourceTimes))
            {
                sourceTimes = SourcesDatabase.LoadLastFetchTimes(station);
                if (sourceTimes.Count == 0)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    _lastFetchTimes[station.Id] = sourceTimes;
                }
            }
            return sourceTimes[source.Id];
        }

        protected void AddKnownSources()
        {
            AddForecastSource(MeteoserviceruSource.Instance);
            AddForecastSource(Rp5ruSource.Instance);
            AddForecastSource(WundergroundSource.Instance);
            AddForecastSource(WorldweatheronlineSource.Instance);
            AddForecastSource(OpenweathermaporgSource.Instance);
            AddForecastSource(WeatheruaSource.Instance);
            AddForecastSource(YrnoSource.Instance);
            AddForecastSource(GismeteoSource.Instance);
            AddForecastSource(ForecastioSource.Instance);
            AddRealSource(AddsMetarSource.Instance);
        }

        internal void FlushFetchTimes(IEnumerable<Station> stations)
        {
            SourcesDatabase.SaveLastFetchTimes(AllDataSources, stations);
        }

        protected void AddForecastSource(ForecastDataSource dataSource)
        {
            if (dataSource != null)
            {
                _forecastSources.Add(dataSource);
            }
        }
        protected void AddRealSource(RealDataSource dataSource)
        {
            if (dataSource != null)
            {
                _realSources.Add(dataSource);
            }
        }
        protected SourcesDirector()
        {
            AddKnownSources();
        }

        #region Singleton implementation

        private static readonly Lazy<SourcesDirector> lazyInstance = new Lazy<SourcesDirector>(() => new SourcesDirector());

        public static SourcesDirector Instance
        {
            get
            {
                return lazyInstance.Value;
            }
        }

        #endregion // Singleton implementation}
    }
}