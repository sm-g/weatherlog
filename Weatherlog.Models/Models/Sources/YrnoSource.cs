using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class YrnoSource : ForecastDataSource
    {
        const string _requestTemplate = "http://api.yr.no/weatherapi/locationforecast/1.8/?lat={0};lon={1}";
        const string _name = "Yr.no";
        const string _id = "yrno";
        const string _api = "http://api.yr.no/weatherapi/documentation";
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
        TimeSpan _newDataPeriod = TimeSpan.FromHours(6);
        TimeSpan _forecastsInterval = TimeSpan.FromHours(3);
        TimeSpan _predictedHours = TimeSpan.FromHours(60);


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
                return String.Format(System.Globalization.CultureInfo.InvariantCulture, _requestTemplate, station.Latitude, station.Longitude);
            else
                throw new NoLatLongException(String.Format("{0} requiers coordinates of station", Id));
        }

        protected override IEnumerable<Forecast> ParseResponse(string response, int offset)
        {
            var xdoc = XDocument.Parse(response);
            return ParseXDocument(xdoc, offset);
        }

        #endregion // ForescastDataSource implementation

        #region Parsing logic

        private List<Forecast> ParseXDocument(XDocument xdoc, int offset)
        {
            var forecasts = new List<Forecast>();
            var lastModelRunTime = xdoc.Root.Element("meta").Elements("model").
                Select<XElement, DateTime>(
                    model => DateTime.Parse(model.Attribute("runended").Value).ToUniversalTime()).
                Max();

            var creationTime = DateTime.SpecifyKind(lastModelRunTime.AddHours(offset), DateTimeKind.Unspecified);

            var fArray = xdoc.Root.Element("product").Elements("time").ToArray();
            int i = 0;
            bool mainForecast;
            List<AbstractParameter> parameters = null;
            DateTime lastValidTime = DateTime.MaxValue; // must be max at begin
            DateTime validTime = DateTime.MinValue;
            while (i < fArray.Length)
            {
                if (i % 3 == 2)
                {
                    i++;
                    continue; // every 3rd forecast in frequent forecasts part contains only 6 hour precipitation
                }
                var xForecast = fArray[i];

                mainForecast = i % 3 == 0; // every 2nd - 3 hour precipitation, 1st - other parameters

                if (mainForecast)
                {
                    parameters = new List<AbstractParameter>();

                    ParseMainXForecast(xForecast.Element("location"), parameters);

                    validTime = ParseXValidTime(xForecast, offset);
                    if (validTime - lastValidTime <= _forecastsInterval)
                    {
                        lastValidTime = validTime;
                    }
                    else
                    {
                        break; // non-frequent forecasts part starts
                    }
                }
                else
                {
                    ParsePrecipXForecast(xForecast.Element("location"), parameters);
                    Forecast forecast = new Forecast(parameters, creationTime, validTime);
                    forecasts.Add(forecast);
                }

                i++;
            }
            return forecasts;
        }

        private static DateTime ParseXValidTime(XElement xForecast, int offset)
        {
            return DateTime.SpecifyKind(DateTime.Parse(
                 xForecast.Attribute("to").Value).ToUniversalTime().
                 AddHours(offset), DateTimeKind.Unspecified);
        }

        private void ParsePrecipXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            parameters.Add(new PrecipitationAmount(
               xForecast.Element("precipitation").Attribute("value").Value));
        }

        private static void ParseMainXForecast(XElement xForecast, List<AbstractParameter> parameters)
        {
            parameters.Add(Temperature.FromDouble(
                xForecast.Element("temperature").Attribute("value").Value));

            parameters.Add(WindDirection.FromDouble(
                xForecast.Element("windDirection").Attribute("deg").Value));

            parameters.Add(WindSpeed.FromDouble(
                xForecast.Element("windSpeed").Attribute("mps").Value));

            parameters.Add(Humidity.FromDouble(
                xForecast.Element("humidity").Attribute("value").Value));

            parameters.Add(Pressure.FromHpa(
                xForecast.Element("pressure").Attribute("value").Value));

            parameters.Add(Cloudiness.FromDouble(
                xForecast.Element("cloudiness").Attribute("percent").Value));
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<YrnoSource> lazyInstance = new Lazy<YrnoSource>(() => new YrnoSource());

        public static ForecastDataSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private YrnoSource()
        {
        }

        #endregion // Singleton implementation
    }
}
