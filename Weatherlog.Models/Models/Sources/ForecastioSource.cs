using ForecastIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Weatherlog.Models.Parameters;

namespace Weatherlog.Models.Sources
{
    public sealed class ForecastioSource : ForecastDataSource
    {
        const string _requestTemplate = "https://api.forecast.io/forecast/54c5072a9f89794f01f719d9afe2997e/{0},{1}?units=si&exclude=currently,minutely,daily,alerts,flags";
        const string _name = "Forecast.io";
        const string _id = "forecastio";
        const string _api = "https://developer.forecast.io/docs/v2";
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
        TimeSpan _newDataPeriod = TimeSpan.FromHours(2);
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

        public override bool IsUtcOffsetInResponse
        {
            get { return true; }
        }

        protected override string GetQuery(Station station)
        {
            if (station.HasLatLongCoordinates)
                return String.Format(System.Globalization.CultureInfo.InvariantCulture, _requestTemplate, station.Latitude, station.Longitude);
            else
                throw new NoLatLongException(String.Format("{0} requiers coordinates of station", Id));
        }

        protected override IEnumerable<Forecast> ParseResponse(string response, int utcOffset)
        {
            ForecastIOResponse dataObject = JsonConvert.DeserializeObject<ForecastIOResponse>(response);
            return ConvertResponceToForecasts(dataObject);
        }

        #endregion // ForescastDataSource implementation

        #region Parsing logic

        private DateTime GetForecastUTCCreationTime(DateTime fetchedUtcTime)
        {
            var hour = fetchedUtcTime.Hour - 1;
            return fetchedUtcTime.Date.AddHours(hour);
        }

        private List<Forecast> ConvertResponceToForecasts(ForecastIOResponse dataObject)
        {
            var forecasts = new List<Forecast>();

            var creationTime = DateTime.SpecifyKind(GetForecastUTCCreationTime(DateTime.UtcNow).AddHours(
                dataObject.offset), DateTimeKind.Unspecified);

            foreach (var hourForecast in dataObject.hourly.data)
            {
                var parameters = new List<AbstractParameter>();

                ConvertHourForecast(hourForecast, parameters);

                var validTime = ParseXValidTime(hourForecast, (int)dataObject.offset);
                Forecast forecast = new Forecast(parameters, creationTime, validTime);
                forecasts.Add(forecast);
            }
            return forecasts;
        }

        private static DateTime ParseXValidTime(HourForecast hourForecast, int offset)
        {
            var validTime = DateTime.SpecifyKind(ForecastIO.Extensions.Extensions.ToDateTime(
                 hourForecast.time).AddHours(offset), DateTimeKind.Unspecified);
            return validTime;
        }

        private static void ConvertHourForecast(HourForecast hourForecast, List<AbstractParameter> parameters)
        {
            parameters.Add(new Temperature(
                hourForecast.temperature));

            parameters.Add(Pressure.FromHpa(
                hourForecast.pressure));

            parameters.Add(new PrecipitationAmount(
                hourForecast.precipIntensity));

            parameters.Add(Humidity.FromPart(
                hourForecast.humidity));

            parameters.Add(Cloudiness.FromPart(
                hourForecast.cloudCover));

            parameters.Add(new WindDirection(
                hourForecast.windBearing));

            parameters.Add(new WindSpeed(
                hourForecast.windSpeed));
        }

        #endregion

        #region Singleton implementation

        private static readonly Lazy<ForecastioSource> lazyInstance = new Lazy<ForecastioSource>(() => new ForecastioSource());

        public static ForecastDataSource Instance
        {
            get { return lazyInstance.Value; }
        }

        private ForecastioSource()
        {
        }

        #endregion // Singleton implementation
    }
}
