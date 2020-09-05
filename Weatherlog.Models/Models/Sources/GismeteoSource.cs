using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class GismeteoSource : ForecastDataSource
    {
        const string _requestTemplate = "http://informer.gismeteo.ru/xml/{0}.xml";
        const string _name = "Gismeteo";
        const string _id = "gismeteo";
        const string _api = "http://informer.gismeteo.ru/getcode/xml.php";
        List<string> _parameters = new List<string>()
        {
            Temperature.TypeName,
            Pressure.TypeName,
            Cloudiness.TypeName,
            Humidity.TypeName,
            WindDirection.TypeName,
            WindSpeed.TypeName,
            PrecipitationKind.TypeName
        };
        DateTime _lastSuccessfullRequest = DefaultLastFetchTime;
        TimeSpan _newDataPeriod = TimeSpan.FromHours(6);
        TimeSpan _predictedHours = TimeSpan.FromHours(24);

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

        const int maxClouds = 3;
        const int maxWindDir = 8;

        private DateTime GetForecastUTCCreationTime(DateTime fetchedUtcTime)
        {
            var hour = fetchedUtcTime.Hour;

            //23.30, 5.30, 11.30, 17.30
            if (hour < 6)
                return fetchedUtcTime.Date.AddHours(-0.5);
            if (hour < 12)
                return fetchedUtcTime.Date.AddHours(5.5);
            if (hour < 18)
                return fetchedUtcTime.Date.AddHours(11.5);
            return fetchedUtcTime.Date.AddHours(17.5);
        }

        private List<Forecast> ParseXDocument(XDocument xdoc, int utcOffset)
        {
            var forecasts = new List<Forecast>(4);
            var creationTime = DateTime.SpecifyKind(GetForecastUTCCreationTime(DateTime.UtcNow).AddHours(utcOffset), DateTimeKind.Unspecified);

            foreach (var xForecast in xdoc.Descendants("FORECAST"))
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
            var validTime = new DateTime(
                int.Parse(xForecast.Attribute("year").Value),
                int.Parse(xForecast.Attribute("month").Value),
                int.Parse(xForecast.Attribute("day").Value),
                int.Parse(xForecast.Attribute("hour").Value),
                0, 0);
            return validTime;
        }

        private static void ParseXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            var xPhenomena = xForecast.Element("PHENOMENA");

            parameters.Add(Cloudiness.FromRange(
                xPhenomena.Attribute("cloudiness").Value, maxClouds));
            parameters.Add(PrecipitationKind.FromGismeteoBased(
                xPhenomena.Attribute("precipitation").Value));

            var xPres = xForecast.Element("PRESSURE");

            parameters.Add(new Pressure(
                xPres.Attribute("max").Value));
            parameters.Add(new Pressure(
                xPres.Attribute("min").Value));

            var xTemp = xForecast.Element("TEMPERATURE");

            parameters.Add(new Temperature(
                xTemp.Attribute("max").Value));
            parameters.Add(new Temperature(
                xTemp.Attribute("min").Value));

            var xWind = xForecast.Element("WIND");

            parameters.Add(WindDirection.FromRange(
                xWind.Attribute("direction").Value, maxWindDir));
            parameters.Add(new WindSpeed(
                xWind.Attribute("max").Value));
            parameters.Add(new WindSpeed(
                xWind.Attribute("min").Value));

            var xHum = xForecast.Element("RELWET");

            parameters.Add(new Humidity(
                xHum.Attribute("max").Value));
            parameters.Add(new Humidity(
                xHum.Attribute("min").Value));
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<GismeteoSource> lazyInstance = new Lazy<GismeteoSource>(() => new GismeteoSource());

        public static ForecastDataSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private GismeteoSource()
        {
        }

        #endregion // Singleton implementation
    }
}
