using System;

namespace Weatherlog.Models
{
    public class FetchingEndedEventArgs : EventArgs
    {
        public int successfullyFetched;
        public int notFetched;
        public TimeSpan duration;
        public bool isRegular;

        public FetchingEndedEventArgs(int successfullyFetched, int notFetched, TimeSpan duration, bool isRegular)
        {
            this.successfullyFetched = successfullyFetched;
            this.notFetched = notFetched;
            this.duration = duration;
            this.isRegular = isRegular;
        }

    }
}
