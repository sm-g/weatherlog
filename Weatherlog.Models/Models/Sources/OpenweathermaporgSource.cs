using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class OpenweathermaporgSource : ForecastDataSource
    {
        const string _requestTemplate = "http://api.openweathermap.org/data/2.5/forecast?q={0}&mode=xml&units=metric";
        const string _requestLatLonTemplate = "http://api.openweathermap.org/data/2.5/forecast?lat={0}&lon={1}&mode=xml&units=metric";
        const string _name = "Open Weather Map";
        const string _id = "owm";
        const string _api = "http://openweathermap.org/API";
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

        private List<Forecast> ParseXDocument(XDocument xdoc, int utcOffset)
        {
            var forecasts = new List<Forecast>();

            var creationTime = DateTime.Parse(
                xdoc.Root.Element("meta").Element("lastupdate").Value).AddHours(utcOffset);

            foreach (var xForecast in xdoc.Root.Element("forecast").Elements())
            {
                var parameters = new List<AbstractParameter>();

                ParseXForecast(xForecast, parameters);

                var validTime = ParseXValidTime(xForecast, utcOffset);
                Forecast forecast = new Forecast(parameters, creationTime, validTime);
                forecasts.Add(forecast);
            }
            return forecasts;
        }

        private static DateTime ParseXValidTime(XElement xForecast, int utcOffset)
        {
            var validTime = DateTime.Parse(
                 xForecast.Attribute("to").Value).AddHours(utcOffset);
            return validTime;
        }

        private static void ParseXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            parameters.Add(Temperature.FromDouble(
                xForecast.Element("temperature").Attribute("value").Value));

            parameters.Add(Pressure.FromHpa(
                xForecast.Element("pressure").Attribute("value").Value));

            var xPrecipVal = xForecast.Element("precipitation").Attribute("value");
            if (xPrecipVal != null)
                parameters.Add(new PrecipitationAmount(
                    xPrecipVal.Value));
            else
                parameters.Add(new PrecipitationAmount(0));

            parameters.Add(new Humidity(
                xForecast.Element("humidity").Attribute("value").Value));

            parameters.Add(new Cloudiness(
                xForecast.Element("clouds").Attribute("all").Value));

            parameters.Add(WindDirection.FromDouble(
                xForecast.Element("windDirection").Attribute("deg").Value));

            parameters.Add(WindSpeed.FromDouble(
                xForecast.Element("windSpeed").Attribute("mps").Value));
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<OpenweathermaporgSource> lazyInstance = new Lazy<OpenweathermaporgSource>(() => new OpenweathermaporgSource());

        public static ForecastDataSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private OpenweathermaporgSource()
        {
        }

        #endregion // Singleton implementation
    }
}
