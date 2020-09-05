using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class WorldweatheronlineSource : ForecastDataSource
    {
        const string _requestTemplate = "http://api.worldweatheronline.com/free/v1/weather.ashx?q={0}&format=xml&extra=localObsTime&num_of_days=5&cc=no&key=a3xnndjcumsfb397qzp36d5r";
        const string _requestLatLonTemplate = "http://api.worldweatheronline.com/free/v1/weather.ashx?q={0},{1}&format=xml&extra=localObsTime&num_of_days=5&cc=no&key=a3xnndjcumsfb397qzp36d5r";
        const string _name = "World Weather Online";
        const string _id = "wwo";
        const string _api = "http://developer.worldweatheronline.com/documentation";
        List<string> _parameters = new List<string>()
        {
            Temperature.TypeName,
            WindDirection.TypeName,
            WindSpeed.TypeName,
            PrecipitationAmount.TypeName
        };
        DateTime _lastSuccessfullRequest = DefaultLastFetchTime;
        TimeSpan _newDataPeriod = TimeSpan.FromHours(4);
        TimeSpan _predictedHours = TimeSpan.FromHours(5 * 24);

        #region ForescastDataSource implementation

        #region Properties

        public override string Name
        {
            get { return _name; }
        }
        public override string Id
        {
            get { return _id; }
        }
        public override string ApiDescriptionUri
        {
            get { return _api; }
        }
        public override DateTime LastSuccessfullFetching
        {
            get { return _lastSuccessfullRequest; }
            protected set { _lastSuccessfullRequest = value; }
        }
        public override TimeSpan UpdatingDataPeriod
        {
            get { return _newDataPeriod; }
        }
        public override IEnumerable<string> Parameters
        {
            get { return _parameters; }
        }
        public override TimeSpan PredictionDepth
        {
            get { return _predictedHours; }
        }


        #endregion // Properties


        protected override string GetQuery(Station station)
        {
            if (station.HasLatLongCoordinates)
                return String.Format(System.Globalization.CultureInfo.InvariantCulture, _requestLatLonTemplate, station.Latitude, station.Longitude);
            return String.Format(_requestTemplate, station.Name);
        }

        protected override IEnumerable<Forecast> ParseResponse(string response, int utcOffset)
        {
            var xdoc = XDocument.Parse(response);
            return ParseXDocument(xdoc, utcOffset);
        }

        #endregion // ForescastDataSource implementation

        #region Parsing logic

        private DateTime GetForecastUTCCreationTime(DateTime fetchedUtcTime)
        {
            // guess...
            var hour = fetchedUtcTime.Hour - UpdatingDataPeriod.Hours;
            return fetchedUtcTime.Date.AddHours(hour);
        }

        private List<Forecast> ParseXDocument(XDocument xdoc, int utcOffset)
        {
            var forecasts = new List<Forecast>();

            var creationTime = DateTime.SpecifyKind(GetForecastUTCCreationTime(DateTime.UtcNow).AddHours(utcOffset), DateTimeKind.Unspecified);

            foreach (var xForecast in xdoc.Root.Elements("weather"))
            {
                var parameters = new List<AbstractParameter>();

                ParseXForecast(xForecast, parameters);

                var validTime = ParseXValidTime(xForecast);

                Forecast forecast = new Forecast(parameters, creationTime, validTime);
                forecasts.Add(forecast);
            }
            return forecasts;
        }

        private static DateTime ParseXValidTime(XElement xForecast)
        {
            var validTime = DateTime.Parse(
                 xForecast.Element("date").Value);
            return validTime;
        }

        private static void ParseXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            parameters.Add(new Temperature(
                xForecast.Element("tempMaxC").Value));
            parameters.Add(new Temperature(
                xForecast.Element("tempMinC").Value));

            parameters.Add(WindSpeed.FromKmph(
                xForecast.Element("windspeedKmph").Value));

            parameters.Add(new WindDirection(
                xForecast.Element("winddirDegree").Value));

            parameters.Add(new PrecipitationAmount(
                xForecast.Element("precipMM").Value));
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<WorldweatheronlineSource> lazyInstance = new Lazy<WorldweatheronlineSource>(() => new WorldweatheronlineSource());

        public static ForecastDataSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private WorldweatheronlineSource()
        {
        }

        #endregion // Singleton implementation
    }
}
