using System;
using Weatherlog.Models.Sources;

namespace Weatherlog.Models
{
    public class FetchingSucceededEventArgs : EventArgs
    {
        public Station station;
        public IAbstractDataSource source;
        public DateTime time;

        public FetchingSucceededEventArgs(Station station, IAbstractDataSource source, DateTime time)
        {
            this.station = station;
            this.source = source;
            this.time = time;
        }

    }
}
