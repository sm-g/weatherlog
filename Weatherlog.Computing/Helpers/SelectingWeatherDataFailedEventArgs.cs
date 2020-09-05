using System;

namespace Weatherlog.Computing
{
    public class SelectingWeatherDataFailedEventArgs : EventArgs
    {
        public string message;

        public SelectingWeatherDataFailedEventArgs(string message)
        {
            this.message = message;
        }

    }
}
