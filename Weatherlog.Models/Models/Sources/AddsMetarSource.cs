using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class AddsMetarSource : RealDataSource
    {
        const string _requestTemplateBase = "http://aviationweather.gov/adds/dataserver_current/httpparam?dataSource=metars&requestType=retrieve&format=xml";
        const string _requestLatLonPart = "&minLat={0}&minLon={1}&maxLat={2}&maxLon={3}";
        const string _requestIcaoPart = "&stationString={0}";
        const string _requestStartEndPart = "&startTime={0}&endTime={1}";
        const string _name = "Aviation Digital Data Service";
        const string _id = "adds";
        const string _api = "http://aviationweather.gov/adds/dataserver";
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
        TimeSpan _newDataPeriod = TimeSpan.FromHours(0.5);

        #region RealDataSource implementation

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

        #endregion // Properties

        protected override string GetQuery(Station station)
        {
            var time = String.Format(_requestStartEndPart, GetLastFetchTime(station).ToString("yyyy-MM-ddTHH:mm:ssZ"), DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            if (station.HasIcaoCode)
                return String.Format(_requestTemplateBase + _requestIcaoPart, station.Icao) + time;

            if (!station.HasLatLongCoordinates)
                throw new NoLatLongException(String.Format("{0} requiers coordinates of station", Id));

            var minLat = (station.Latitude - 1).ToString(CultureInfo.InvariantCulture);
            var maxLat = (station.Latitude + 1).ToString(CultureInfo.InvariantCulture);
            var minLon = (station.Longitude - 1).ToString(CultureInfo.InvariantCulture);
            var maxLon = (station.Longitude + 1).ToString(CultureInfo.InvariantCulture);

            return String.Format(CultureInfo.InvariantCulture, _requestTemplateBase + _requestLatLonPart, minLat, minLon, maxLat, maxLon) + time;
        }

        protected override IEnumerable<Forecast> ParseResponse(string response, int utcOffset)
        {
            var xdoc = XDocument.Parse(response);
            return ParseXDocument(xdoc, utcOffset);
        }

        #endregion // RealDataSource implementation

        #region Parsing logic

        private List<Forecast> ParseXDocument(XDocument xdoc, int utcOffset)
        {
            var forecasts = new List<Forecast>();
            var xSubRoot = xdoc.Root.Element("data");
            if (xSubRoot != null)
                foreach (var xForecast in xSubRoot.Elements("METAR"))
                {
                    var parameters = new List<AbstractParameter>();

                    ParseXForecast(xForecast, parameters);

                    var time = ParseXTime(xForecast, utcOffset);

                    Forecast forecast = new Forecast(parameters, time, time);
                    forecasts.Add(forecast);
                }
            return forecasts;
        }

        private static DateTime ParseXTime(XElement xForecast, int offset)
        {
            return DateTime.SpecifyKind(DateTime.Parse(
                     xForecast.Element("observation_time").Value).ToUniversalTime().
                    AddHours(offset), DateTimeKind.Unspecified);
        }

        private static void ParseXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            var temp = Temperature.FromDouble(
                xForecast.Element("temp_c").Value);

            parameters.Add(temp);

            parameters.Add(WindSpeed.FromKnot(
                xForecast.Element("wind_speed_kt").Value));

            parameters.Add(new WindDirection(
                xForecast.Element("wind_dir_degrees").Value));

            var skyDescription = ParseXSky(xForecast);
            parameters.Add(Cloudiness.FromMetarReportInFt(
                skyDescription));

            parameters.Add(Pressure.FromInhg(
                xForecast.Element("altim_in_hg").Value));

            var duePoint = Temperature.FromDouble(
                xForecast.Element("dewpoint_c").Value);

            parameters.Add(Humidity.FromDewPoint(temp, duePoint));

            var xWx = xForecast.Element("wx_string");
            if (xWx != null)
            {
                parameters.Add(PrecipitationKind.FromMetar(
                    xWx.Value));
            }
            else
            {
                parameters.Add(new PrecipitationKind(0));
            }
        }

        private static List<CloudMetarDescription> ParseXSky(XElement xForecast)
        {
            var xSkyConditions = xForecast.Elements("sky_condition");
            var skyDesription = new List<CloudMetarDescription>(3);
            foreach (var xSky in xSkyConditions)
            {
                var cover = xSky.Attribute("sky_cover").Value;
                var bases = xSky.Attributes("cloud_base_ft_agl");
                int height = 0;
                if (bases.Count() > 0)
                {
                    height = int.Parse(bases.First().Value);
                }
                skyDesription.Add(new CloudMetarDescription(cover, height));
            }
            return skyDesription;
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<AddsMetarSource> lazyInstance = new Lazy<AddsMetarSource>(() => new AddsMetarSource());

        public static AddsMetarSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private AddsMetarSource()
        {
        }

        #endregion // Singleton implementation
    }
}
