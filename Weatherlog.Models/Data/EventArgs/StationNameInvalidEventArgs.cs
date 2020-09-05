using System;

namespace Weatherlog.Data
{
    public class StationNameInvalidEventArgs : EventArgs
    {
        public string stationName;

        public StationNameInvalidEventArgs(string stationName)
        {
            this.stationName = stationName;
        }

    }
}
