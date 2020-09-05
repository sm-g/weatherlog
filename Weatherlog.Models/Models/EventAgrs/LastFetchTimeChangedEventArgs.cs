using System;
using Weatherlog.Models.Sources;

namespace Weatherlog.Models
{
    public class LastFetchTimeChangedEventArgs : EventArgs
    {
        public Station station;
        public IAbstractDataSource source;

        public LastFetchTimeChangedEventArgs(IAbstractDataSource source, Station station)
        {
            this.station = station;
            this.source = source;
        }

    }
}
