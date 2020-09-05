using System;
using System.Collections.Generic;
using Weatherlog.Models;
using Weatherlog.Models.Sources;

namespace Weatherlog.Data
{
    public class LoadingFailedEventArgs : EventArgs
    {
        public Station station;
        public IAbstractDataSource source;
        public string message;
        public IEnumerable<DateTime> dates;

        public LoadingFailedEventArgs(Station station, IAbstractDataSource source, IEnumerable<DateTime> dates, string message)
        {
            this.station = station;
            this.source = source;
            this.message = message;
            this.dates = dates;
        }

    }
}
