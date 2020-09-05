using System;
using System.Collections.Generic;
using Weatherlog.Data;

namespace Weatherlog.Models
{
    public class StationsDirector
    {
        private List<Station> _stations;

        public IEnumerable<Station> Stations
        {
            get
            {
                return _stations;
            }
        }

        public void AddStation(Station station)
        {
            if (station != null)
            {
                _stations.Add(station);
                StationsDatabase.Save(_stations);
            }
        }

        public void LoadStations()
        {
            _stations = new List<Station>(StationsDatabase.Load());
        }

        protected StationsDirector()
        {
            LoadStations();
        }

        #region Singleton implementation

        private static readonly Lazy<StationsDirector> lazyInstance = new Lazy<StationsDirector>(() => new StationsDirector());

        public static StationsDirector Instance
        {
            get { return lazyInstance.Value; }
        }

        #endregion // Singleton implementation

    }
}
