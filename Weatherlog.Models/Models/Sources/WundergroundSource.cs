using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class WundergroundSource : ForecastDataSource
    {
        const string _requestTemplate = "http://api.wunderground.com/api/8b697cbdb095ca8f/hourly/q/{0}.xml";
        const string _requestLatLonTemplate = "http://api.wunderground.com/api/8b697cbdb095ca8f/hourly/q/{0},{1}.xml";
        const string _name = "Weather Underground";
        const string _id = "wunderground";
        const string _api = "http://www.wunderground.com/weather/api/d/docs";
        List<string> _parameters = new List<string>()
        {
            Temperature.TypeName,
            Pressure.TypeName,
            Cloudiness.TypeName,
            Humidity.TypeName,
            WindDirection.TypeName,
            WindSpeed.TypeName,
            PrecipitationAmount.TypeName
        };
        DateTime _lastSuccessfullRequest = DefaultLastFetchTime;
        TimeSpan _newDataPeriod = TimeSpan.FromHours(3);
        TimeSpan _predictedHours = TimeSpan.FromHours(48);

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

        private static DateTime GetForecastUTCCreationTime(DateTime fetchedUtcTime)
        {
            return fetchedUtcTime.Date.AddHours(fetchedUtcTime.Hour - 1);
        }

        private List<Forecast> ParseXDocument(XDocument xdoc, int utcOffset)
        {
            var forecasts = new List<Forecast>();
            var xSubRoot = xdoc.Root.Element("hourly_forecast");
            var creationTime = DateTime.SpecifyKind(GetForecastUTCCreationTime(DateTime.UtcNow).AddHours(utcOffset), DateTimeKind.Unspecified);

            foreach (var xForecast in xSubRoot.Elements("forecast"))
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
            var xTime = xForecast.Element("FCTTIME");
            var validTime = new DateTime(
                 int.Parse(xTime.Element("year").Value),
                 int.Parse(xTime.Element("mon").Value),
                 int.Parse(xTime.Element("mday").Value),
                 int.Parse(xTime.Element("hour").Value),
                 0, 0);
            return validTime;
        }

        private static void ParseXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            parameters.Add(Temperature.FromDouble(
                xForecast.Element("temp").Element("metric").Value));

            parameters.Add(new Cloudiness(
                xForecast.Element("sky").Value));

            parameters.Add(WindSpeed.FromKmph(
                xForecast.Element("wspd").Element("metric").Value));

            parameters.Add(new WindDirection(
                xForecast.Element("wdir").Element("degrees").Value));

            parameters.Add(Humidity.FromDouble(
                xForecast.Element("humidity").Value));

            parameters.Add(Pressure.FromHpa(
                xForecast.Element("mslp").Element("metric").Value));

            var precip = xForecast.Element("qpf").Element("metric").Value;

            if (precip != "")
                parameters.Add(new PrecipitationAmount(
                    precip));
            else
                parameters.Add(new PrecipitationAmount(
                0));
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<WundergroundSource> lazyInstance = new Lazy<WundergroundSource>(() => new WundergroundSource());

        public static ForecastDataSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private WundergroundSource()
        {
        }

        #endregion // Singleton implementation
    }
}
