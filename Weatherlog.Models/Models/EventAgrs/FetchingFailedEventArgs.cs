using System;
using Weatherlog.Models.Sources;

namespace Weatherlog.Models
{
    public class FetchingFailedEventArgs : EventArgs
    {
        public Station station;
        public IAbstractDataSource source;
        public string message;

        public FetchingFailedEventArgs(Station station, IAbstractDataSource source, string message)
        {
            this.station = station;
            this.source = source;
            this.message = message;
        }

    }
}
