using System;
using Weatherlog.Models;
using Weatherlog.Models.Sources;

namespace Weatherlog.Data
{
    public class SavingFailedEventArgs : EventArgs
    {
        public Station station;
        public IAbstractDataSource source;
        public string message;

        public SavingFailedEventArgs(Station station, IAbstractDataSource source, string message)
        {
            this.station = station;
            this.source = source;
            this.message = message;
        }

    }
}
