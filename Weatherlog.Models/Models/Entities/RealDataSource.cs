using System;
using System.Collections.Generic;

namespace Weatherlog.Models.Sources
{
    public abstract class RealDataSource : IAbstractDataSource
    {
        private Dictionary<Station, DateTime> _stationLastFetchTime = new Dictionary<Station, DateTime>();

        public event EventHandler<LastFetchTimeChangedEventArgs> LastFetchTimeChanged = delegate { };

        #region IAbstractDataSource implementation

        public abstract string Name
        {
            get;
        }

        public abstract string Id
        {
            get;
        }

        public abstract string ApiDescriptionUri
        {
            get;
        }

        public abstract TimeSpan UpdatingDataPeriod
        {
            get;
        }
        public abstract IEnumerable<string> Parameters
        {
            get;
        }
        public abstract DateTime LastSuccessfullFetching
        {
            get;
            protected set;
        }

        #endregion //IAbstractDataSource implementation

        protected static DateTime DefaultLastFetchTime { get { return DateTime.MinValue; } }

        /// <summary>
        /// Determines whether new forecast data for selected station available (as it declared by source).
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public virtual bool IsNewDataReady(Station station)
        {
            return GetLastFetchTime(station) + UpdatingDataPeriod < DateTime.UtcNow;
        }

        public virtual Weather GetWeatherReal(Station station)
        {
            if (!station.HasUtcOffset)
            {
                throw new NoStationOffsetException(Id + " requiers offset in station.");
            }

            string query = GetQuery(station);
            var response = Loader.LoadString(query);

            IEnumerable<Forecast> forecasts;
            try
            {
                forecasts = ParseResponse(response, station.UtcOffset);
            }
            catch (Exception e)
            {
                throw new ResponseParsingException(e.Message, e);
            }

            var result = new Weather(station, this, forecasts);

            var now = DateTime.UtcNow;
            LastSuccessfullFetching = now;
            UpdateLastFetchTime(station, now);
            return result;
        }

        public DateTime GetLastFetchTime(Station station)
        {
            DateTime time;
            if (_stationLastFetchTime.TryGetValue(station, out time))
            {
                return time;
            }
            else
            {
                return SourcesDirector.Instance.GetStationLastFetchTime(this, station);
            }
        }

        protected abstract string GetQuery(Station station);

        protected abstract IEnumerable<Forecast> ParseResponse(string response, int utcOffset);

        protected void UpdateLastFetchTime(Station station, DateTime when)
        {
            if (!_stationLastFetchTime.ContainsKey(station))
            {
                _stationLastFetchTime.Add(station, when);
            }
            else
            {
                _stationLastFetchTime[station] = when;
            }
            OnLastFetchTimeChanged(station);
        }

        private void OnLastFetchTimeChanged(Station station)
        {
            //LastFetchTimeChanged(this, new LastFetchTimeChangedEventArgs(this, station));
        }
    }
}
