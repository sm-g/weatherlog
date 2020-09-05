using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Weatherlog.Data;
using Weatherlog.Models;
using Weatherlog.Models.Parameters;
using Weatherlog.Models.Sources;

namespace Weatherlog.Computing
{
    public enum GivenForecastTimeKind
    {
        Created,
        Valid
    }

    public static class WeatherDataSelecter
    {
        private static int plotGroupKey = 0;

        public static event EventHandler<SelectingWeatherDataFailedEventArgs> SelectingWeatherDataFailed = delegate { };

        public static Dictionary<string, PlotData> GetGroupedByParameterAccuracyPlots(AccuracyRequest request)
        {
            var result = new Dictionary<string, PlotData>();
            var loadedForecasts = new Dictionary<IAbstractDataSource, IEnumerable<Forecast>>();

            foreach (var source in request.Sources)
            {
                loadedForecasts.Add(
                    source, ForecastsDatabase.Load(
                        request.Station,
                        source,
                        GetValidTimeDates(request.Date, GivenForecastTimeKind.Valid, source)));
            }

            int plotsKey = GeneratePlotGroupKey();

            int hours = request.AccuracyComparison == AccuracyComparison.HalfDay ? 12 : 24;
            DateTime date = request.Date;

            var realSources = request.Sources.Where(s => s is RealDataSource).Cast<RealDataSource>();
            if (realSources.Count() == 0)
            {
                realSources = SourcesDirector.Instance.RealDataSources;
            }
            if (realSources.Count() == 0)
            {
                OnSelectingFailed("Не выбран источник реальных данных.");
                return result;
            }
            RealDataSource realSource = realSources.First();

            try
            {
                foreach (var parameterType in request.ParameterTypes)
                {
                    var dict = (from source in request.Sources
                                where source is ForecastDataSource
                                where source.Parameters.Contains(parameterType)
                                let allSeries = from f in FilterForecastsForSingleDate(loadedForecasts[source], TimeSpan.FromHours(hours), date)
                                                select GetSeries(f.ToIEnumerable(), GivenForecastTimeKind.Created, parameterType, f.CreationTime)
                                where allSeries.Count() > 0
                                select new
                                {
                                    Key = source,
                                    Value = allSeries.MergeSeries()
                                }).ToDictionary(item => item.Key, item => item.Value);

                    var real = from f in loadedForecasts[realSource]
                               group f by GetForecastGroupingTime(f, GivenForecastTimeKind.Valid) into fByValidTime
                               where IsSuitableDateTime(fByValidTime.Key, date, RequestTimeDeterminateness.AllDay)
                               select GetSeries(fByValidTime.ToList(), GivenForecastTimeKind.Created, parameterType, fByValidTime.Key);

                    dict.Add(realSource, real.MergeSeries());
                    result.Add(parameterType, new PlotData(plotsKey, parameterType, dict));
                }
            }
            catch (Exception e)
            {
                OnSelectingFailed(e.Message);
            }

            return result;
        }

        public static Dictionary<string, PlotData> GetGroupedByParameterForecastPlots(ForecastViewerRequest request)
        {
            var result = new Dictionary<string, PlotData>();
            var loadedForecasts = new Dictionary<IAbstractDataSource, IEnumerable<Forecast>>();

            foreach (var source in request.Sources)
            {
                loadedForecasts.Add(
                    source, ForecastsDatabase.Load(
                        request.Station,
                        source,
                        GetValidTimeDates(request.DateTime, request.GivenTime, source)));
            }

            int plotsKey = GeneratePlotGroupKey();

            foreach (var parameterType in request.ParameterTypes)
            {
                var sourceSeriesDict = GetSeriesBySourceDictionary(request, loadedForecasts, parameterType);
                result.Add(parameterType, new PlotData(plotsKey, parameterType, sourceSeriesDict));
            }

            return result;
        }

        private static Dictionary<IAbstractDataSource, IEnumerable<ParameterTimeSeries>> GetSeriesBySourceDictionary(
            ForecastViewerRequest request, Dictionary<IAbstractDataSource, IEnumerable<Forecast>> loadedForecasts, string parameterType)
        {
            var result = new Dictionary<IAbstractDataSource, IEnumerable<ParameterTimeSeries>>();
            try
            {
                result =
                   (from source in request.Sources
                    where source.Parameters.Contains(parameterType)
                    let allSeries = from f in loadedForecasts[source]
                                    group f by GetForecastGroupingTime(f, request.GivenTime) into fByTime
                                    where IsSuitableDateTime(fByTime.Key, request.DateTime, request.TimeDeterminateness)
                                    select GetSeries(fByTime.ToList(), request.GivenTime, parameterType, fByTime.Key)
                    select new
                    {
                        Key = source,
                        Value = allSeries
                    }).ToDictionary(item => item.Key, item => item.Value);
            }
            catch (Exception e)
            {
                OnSelectingFailed(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Returns forecasts valid for specified date, created as late as possible, but not earlier than "date - distanceFromDate"
        /// </summary>
        private static IEnumerable<Forecast> FilterForecastsForSingleDate(IEnumerable<Forecast> forecasts, TimeSpan distanceFromDate, DateTime date)
        {
            var q = from f in forecasts
                    where IsSuitableDateTime(f.ValidTime, date, RequestTimeDeterminateness.AllDay)
                    group f by f.CreationTime into fByCreationTime
                    where date - fByCreationTime.Key >= distanceFromDate
                    select new
                    {
                        Time = fByCreationTime.Key,
                        Forecasts = fByCreationTime
                    };
            if (q.Count() == 0)
                return new List<Forecast>();

            return q.OrderBy(w => w.Time).Last().Forecasts.ToList();
        }

        /// <summary>
        /// Retrieves series of values of specific parameter (their maximums in each forecast by default).
        /// </summary>
        internal static ParameterTimeSeries GetSeries(IEnumerable<Forecast> forecasts, GivenForecastTimeKind givenTimeMode, string parameterType, DateTime groupTime, bool minValues = false)
        {
            var dict =
               (from f in forecasts.DistinctBy(f => GetForecastTargetTime(f, givenTimeMode), null)
                let p = SelectParameter(f, parameterType, minValues)
                where p != null
                select new
                {
                    Key = GetForecastTargetTime(f, givenTimeMode),
                    Value = p.Value
                }).ToDictionary(i => i.Key, i => i.Value);

            return new ParameterTimeSeries(dict, parameterType, groupTime, givenTimeMode);
        }

        /// <summary>
        /// Opposite to given time.
        /// </summary>
        private static DateTime GetForecastTargetTime(Forecast f, GivenForecastTimeKind givenTimeMode)
        {
            switch (givenTimeMode)
            {
                case GivenForecastTimeKind.Created:
                    return f.ValidTime;

                case GivenForecastTimeKind.Valid:
                default:
                    return f.CreationTime;
            }
        }

        /// <summary>
        /// As given time.
        /// </summary>
        private static DateTime GetForecastGroupingTime(Forecast f, GivenForecastTimeKind givenTimeMode)
        {
            switch (givenTimeMode)
            {
                case GivenForecastTimeKind.Created:
                    return f.CreationTime;

                case GivenForecastTimeKind.Valid:
                default:
                    return f.ValidTime;
            }
        }

        private static DateTime[] GetValidTimeDates(DateTime date, GivenForecastTimeKind mode, IAbstractDataSource source)
        {
            DateTime[] validTimeDates = null;

            if (mode == GivenForecastTimeKind.Valid || source is RealDataSource)
            {
                validTimeDates = new DateTime[1] { date };
            }
            else if (mode == GivenForecastTimeKind.Created)
            {
                int daysForward = (int)Math.Ceiling(((ForecastDataSource)source).PredictionDepth.TotalDays);
                validTimeDates = new DateTime[daysForward];
                for (int i = 0; i < daysForward; i++)
                {
                    validTimeDates[i] = date.AddDays(i);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("mode", "Unknown kind of given forecast time.");
            }

            return validTimeDates;
        }

        private static bool IsSuitableDateTime(DateTime inForecast, DateTime inRequest, RequestTimeDeterminateness mode)
        {
            bool hours;
            switch (mode)
            {
                case RequestTimeDeterminateness.DateAndHour:
                    hours = inForecast.Hour == inRequest.Hour;
                    break;

                default:
                    hours = true;
                    break;
            }
            return inForecast.Date == inRequest.Date && hours;
        }

        private static T MaximalParameter<T>(IEnumerable<Forecast> forecasts) where T : AbstractParameter, IComparable
        {
            var query = SelectManyParameters<T>(forecasts);
            return (T)query.Max();
        }

        private static AbstractParameter MaximalParameter(IEnumerable<Forecast> forecasts, string parameterType)
        {
            var query = SelectManyParameters(forecasts, parameterType);
            return query.Max();
        }

        private static IEnumerable<AbstractParameter> SelectManyParameters<T>(IEnumerable<Forecast> forecasts, bool minValue = false) where T : AbstractParameter, IComparable
        {
            return
                from f in forecasts
                select SelectParameter<T>(f, minValue);
        }

        private static IEnumerable<AbstractParameter> SelectManyParameters(IEnumerable<Forecast> forecasts, string parameterType, bool minValue = false)
        {
            return
                from f in forecasts
                select SelectParameter(f, parameterType, minValue);
        }

        private static AbstractParameter SelectParameter<T>(Forecast forecast, bool minValue) where T : AbstractParameter, IComparable
        {
            return minValue ? SelectParameters<T>(forecast).Min() : SelectParameters<T>(forecast).Max();
        }

        private static AbstractParameter SelectParameter(Forecast forecast, string parameterType, bool minValue)
        {
            return minValue ? SelectParameters(forecast, parameterType).Min() : SelectParameters(forecast, parameterType).Max();
        }

        private static IEnumerable<AbstractParameter> SelectParameters<T>(Forecast forecast) where T : AbstractParameter
        {
            return
                from param in forecast.Parameters
                where param.GetType() == typeof(T)
                select param;
        }

        private static IEnumerable<AbstractParameter> SelectParameters(Forecast forecast, string parameterType)
        {
            return
                from param in forecast.Parameters
                where param.ShortTypeName == parameterType
                select param;
        }

        private static int GeneratePlotGroupKey()
        {
            if (plotGroupKey < int.MaxValue)
                return ++plotGroupKey;
            else
                return plotGroupKey = 0;
        }

        private static void OnSelectingFailed(string message)
        {
            SelectingWeatherDataFailed(null, new SelectingWeatherDataFailedEventArgs(message));
        }
    }
}