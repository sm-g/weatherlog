using System;
using System.Collections.Generic;

namespace Weatherlog.Models.Sources
{
    sealed class DummySource : ForecastDataSource
    {
        #region ForescastDataSource implementation

        public override string ApiDescriptionUri
        {
            get { return "Fake forecast data source."; }
        }

        public override string Name
        {
            get { return "Dummy"; }
        }

        public override string Id
        {
            get { return "dummy"; }
        }

        public override DateTime LastSuccessfullFetching
        {
            get { return DateTime.MinValue; }
            protected set { }
        }
        public override TimeSpan UpdatingDataPeriod
        {
            get { return TimeSpan.FromHours(24); }
        }

        public override IEnumerable<string> Parameters
        {
            get { return new List<string>(); }
        }

        public override bool IsNewDataReady(Station station)
        {
            return true;
        }
        public override TimeSpan PredictionDepth
        {
            get { return new TimeSpan(1); }
        }

        protected override string GetQuery(Station station)
        {
            return "empty";
        }

        protected override IEnumerable<Forecast> ParseResponse(string response, int offset)
        {
            return new List<Forecast>();
        }

        #endregion // ForescastDataSource implementation
    }
}
