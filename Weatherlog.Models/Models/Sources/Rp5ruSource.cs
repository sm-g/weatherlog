using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class Rp5ruSource : ForecastDataSource
    {
        const string _requestTemplate = "http://rp5.ru/xml/{0}/00000/en";
        const string _name = "Расписание погоды";
        const string _id = "rp5ru";
        const string _api = "http://rp5.ru/docs/xml/ru";
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
        TimeSpan _newDataPeriod = TimeSpan.FromHours(12);
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

        public override bool IsNewDataReady(Station station)
        {
            var hourNow = DateTime.UtcNow.Hour;
            if (hourNow == 4 || hourNow == 16)
            {
                // skip this hour - data updates after 04:00 and 16:00 UTC
                return false;
            }
            return GetLastFetchTime(station) + UpdatingDataPeriod < DateTime.UtcNow;
        }

        public override bool IsUtcOffsetInResponse
        {
            get { return true; }
        }

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
            return ParseXDocument(xdoc);
        }

        #endregion // ForescastDataSource implementation

        #region Parsing logic

        private static DateTime GetForecastUTCCreationTime(DateTime fetchedUtcTime)
        {
            var hour = fetchedUtcTime.Hour;
            if (hour < 4)
                // yesterday at 16
                return fetchedUtcTime.Date.AddDays(-1).AddHours(16);
            if (hour < 16)
                // today at 4
                return fetchedUtcTime.Date.AddHours(4);
            // today at 16
            return fetchedUtcTime.Date.AddHours(16);
        }

        private List<Forecast> ParseXDocument(XDocument xdoc)
        {
            var forecasts = new List<Forecast>(4);
            var xPoint = xdoc.Root.Element("point");
            var creationTime = DateTime.SpecifyKind(GetForecastUTCCreationTime(DateTime.UtcNow).AddHours(
                int.Parse(xPoint.Element("gmt_add").Value)), DateTimeKind.Unspecified);

            foreach (var xForecast in xPoint.Elements("timestep"))
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
                 xForecast.Element("datetime").Value);
            return validTime;
        }

        private static void ParseXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            parameters.Add(new Temperature(
                xForecast.Element("temperature").Value));

            parameters.Add(new Pressure(
                xForecast.Element("pressure").Value));

            parameters.Add(new PrecipitationAmount(
                xForecast.Element("precipitation").Value));

            parameters.Add(new Humidity(
                xForecast.Element("humidity").Value));

            parameters.Add(new Cloudiness(
                xForecast.Element("cloud_cover").Value));

            parameters.Add(WindDirection.FromCode(
                xForecast.Element("wind_direction").Value));

            parameters.Add(new WindSpeed(
                xForecast.Element("wind_velocity").Value));
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<Rp5ruSource> lazyInstance = new Lazy<Rp5ruSource>(() => new Rp5ruSource());

        public static ForecastDataSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private Rp5ruSource()
        {
        }

        #endregion // Singleton implementation
    }
}
