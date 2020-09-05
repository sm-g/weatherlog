using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class WeatheruaSource : ForecastDataSource
    {
        const string _requestTemplate = "http://xml.weather.co.ua/1.2/forecast/{0}?dayf=5";
        const string _name = "Weather.ua";
        const string _id = "weatherua";
        const string _api = "http://www.weather.ua/services/xml";
        List<string> _parameters = new List<string>()
        {
            Temperature.TypeName,
            Pressure.TypeName,
            Cloudiness.TypeName,
            Humidity.TypeName,
            WindDirection.TypeName,
            WindSpeed.TypeName
        };
        DateTime _lastSuccessfullRequest = DefaultLastFetchTime;
        TimeSpan _newDataPeriod = TimeSpan.FromHours(6);
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
            string query;
            if (station.SourceQueryMap.TryGetValue(Id, out query))
                return String.Format(_requestTemplate, query);
            else
                throw new StationQueryNotFoundException(Id + " requiers query string in station.");

        }

        protected override IEnumerable<Forecast> ParseResponse(string response, int utcOffset)
        {
            var xdoc = XDocument.Parse(response);
            return ParseXDocument(xdoc, utcOffset);
        }

        #endregion // ForescastDataSource implementation

        #region Parsing logic

        private List<Forecast> ParseXDocument(XDocument xdoc, int utcOffset)
        {
            var forecasts = new List<Forecast>();

            var creationTime = DateTime.SpecifyKind(DateTime.Parse(
                xdoc.Root.Attribute("last_updated").Value).ToUniversalTime().
                    AddHours(utcOffset), DateTimeKind.Unspecified);

            foreach (var xForecast in xdoc.Root.Element("forecast").Elements())
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
                 xForecast.Attribute("date").Value).AddHours(int.Parse(
                 xForecast.Attribute("hour").Value));
            return validTime;
        }

        private static void ParseXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            parameters.Add(new Cloudiness(
                xForecast.Element("cloud").Value));

            var xPres = xForecast.Element("p");

            parameters.Add(new Pressure(
                xPres.Element("max").Value));
            parameters.Add(new Pressure(
                xPres.Element("min").Value));

            var xTemp = xForecast.Element("t");

            parameters.Add(new Temperature(
                xTemp.Element("max").Value));
            parameters.Add(new Temperature(
                xTemp.Element("min").Value));

            var xWind = xForecast.Element("wind");

            parameters.Add(new WindDirection(
                xWind.Element("rumb").Value));
            parameters.Add(new WindSpeed(
                xWind.Element("max").Value));
            parameters.Add(new WindSpeed(
                xWind.Element("min").Value));

            var xHum = xForecast.Element("hmid");

            parameters.Add(new Humidity(
                xHum.Element("max").Value));
            parameters.Add(new Humidity(
                xHum.Element("min").Value));
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<WeatheruaSource> lazyInstance = new Lazy<WeatheruaSource>(() => new WeatheruaSource());

        public static ForecastDataSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private WeatheruaSource()
        {
        }

        #endregion // Singleton implementation
    }
}
