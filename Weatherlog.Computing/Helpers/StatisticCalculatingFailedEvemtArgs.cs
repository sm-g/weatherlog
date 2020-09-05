using System;

namespace Weatherlog.Computing
{
    public class StatisticCalculatingFailedEventArgs : EventArgs
    {
        public StatisticMethods method;
        public string message;

        public StatisticCalculatingFailedEventArgs(StatisticMethods method, string message)
        {
            this.method = method;
            this.message = message;
        }

    }
}
