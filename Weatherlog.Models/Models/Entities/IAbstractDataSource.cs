using System;
using System.Collections.Generic;

namespace Weatherlog.Models.Sources
{
    public interface IAbstractDataSource
    {
        event EventHandler<LastFetchTimeChangedEventArgs> LastFetchTimeChanged;

        string Name
        {
            get;
        }
        string Id
        {
            get;
        }

        string ApiDescriptionUri
        {
            get;
        }

        TimeSpan UpdatingDataPeriod
        {
            get;
        }

        IEnumerable<string> Parameters
        {
            get;
        }

        DateTime LastSuccessfullFetching
        {
            get;
        }

        /// <summary>
        /// Determines whether new data for selected station available (as it declared by source).
        /// </summary>
        bool IsNewDataReady(Station station);

        DateTime GetLastFetchTime(Station station);
    }
}
