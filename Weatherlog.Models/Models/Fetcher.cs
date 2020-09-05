using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Weatherlog.Data;
using Weatherlog.Models.Sources;

namespace Weatherlog.Models
{
    public class Fetcher
    {
        static TimeSpan defaultCheckingPeriod = TimeSpan.FromHours(0.5);
        TimeSpan _checkingPeriod = defaultCheckingPeriod;
        TimeSpan _nextCheckingAfter;
        readonly object spanLock = new object();
        readonly object fetchLock = new object();
        bool _isChekingGoes = false;
        int fetchingsFailed;
        int fetchingsSucceed;
        ConcurrentBag<Tuple<ForecastDataSource, Station>> failedCases;

        public event EventHandler<FetchingFailedEventArgs> FetchingFailed;
        public event EventHandler<FetchingSucceededEventArgs> FetchingSucceeded;
        public event EventHandler FetchingStarted;
        public event EventHandler<FetchingEndedEventArgs> FetchingEnded;

        public TimeSpan SpanToNextRegularFetch
        {
            get
            {
                lock (spanLock)
                { return _nextCheckingAfter; }
            }
            private set
            {
                lock (spanLock)
                { _nextCheckingAfter = value; }
            }
        }

        public bool IsFetchingGoes
        {
            get
            {
                lock (fetchLock)
                {
                    return _isChekingGoes;
                }
            }
            private set
            {
                lock (fetchLock)
                {
                    _isChekingGoes = value;
                }
            }
        }

        void FetchNewForecasts(Station station, ForecastDataSource source, bool force = false)
        {
            if (!(force || source.IsNewDataReady(station)))
            {
                return;
            }

            Weather weather = null;
            try
            {
                weather = source.GetWeatherForecasts(station);
                OnFetchingSucceeded(station, source, weather.FetchedTime);
            }
            catch (ResponseParsingException e)
            {
                Trace.TraceError(String.Format("Response parsing error when fetching {0} forecast from {1}: {2}", station, source.Id, e.Message));
                OnFetchingFailed(station, source, "ответ сервера не разобран");
                failedCases.Add(new Tuple<ForecastDataSource, Station>(source, station));
            }
            catch (StationQueryNotFoundException e)
            {
                Trace.TraceError(String.Format("Station query not found for {0} in station {1}. {2}", source.Id, station, e.Message));
                OnFetchingFailed(station, source, "требуется строка запроса станции");
            }
            catch (NoLatLongException e)
            {
                Trace.TraceError(String.Format("Coordinates of {0} needed for {1}. {2}", station, source.Id, e.Message));
                OnFetchingFailed(station, source, "требуются координаты станции");
            }
            catch (NoStationOffsetException e)
            {
                Trace.TraceError(String.Format("UTC offset of {0} needed for {1}. {2}", station, source.Id, e.Message));
                OnFetchingFailed(station, source, "требуется часовой пояс станции");
            }

            if (weather != null)
            {
                ForecastsDatabase.Save(weather);
            }
            return;
        }

        void FetchRealData(Station station, RealDataSource source, bool force = false)
        {
            if (!(force || !source.IsNewDataReady(station)))
            {
                return;
            }

            Weather weather = null;
            try
            {
                weather = source.GetWeatherReal(station);
                OnFetchingSucceeded(station, source, weather.FetchedTime);
            }
            catch (ResponseParsingException e)
            {
                Trace.TraceError(String.Format("Response parsing error when fetching {0} forecast from {1}: {2}", station, source.Id, e.Message));
                OnFetchingFailed(station, source, "ответ сервера не разобран");
            }
            catch (NoLatLongException e)
            {
                Trace.TraceError(String.Format("Coordinates of {0} needed for {1}. {2}", station, source.Id, e.Message));
                OnFetchingFailed(station, source, "требуются координаты станции");
            }
            catch (NoStationOffsetException e)
            {
                Trace.TraceError(String.Format("UTC offset of {0} needed for {1}. {2}", station, source.Id, e.Message));
                OnFetchingFailed(station, source, "требуется часовой пояс станции");
            }

            if (weather != null)
            {
                ForecastsDatabase.Save(weather);
            }
            return;
        }

        public void MakeRegularFetching(IEnumerable<Station> stations)
        {
            if (!IsFetchingGoes)
            {
                OnFetchingStarted();
                failedCases = new ConcurrentBag<Tuple<ForecastDataSource, Station>>();
                fetchingsSucceed = 0;
                fetchingsFailed = 0;

                Stopwatch sw = new Stopwatch();
                sw.Restart();

                StartRegularFetchingTasks(stations);

                // repeat for internet problems case, successfully fetched stations-sources are not affected
                if (fetchingsFailed > 0)
                {
                    fetchingsFailed = 0;
                    foreach (var pair in failedCases)
                    {
                        FetchNewForecasts(pair.Item2, pair.Item1);
                    }
                }
                sw.Stop();

                SpanToNextRegularFetch = _checkingPeriod - sw.Elapsed;
                OnFetchingEnded(fetchingsSucceed, fetchingsFailed, sw.Elapsed, true);

                SourcesDirector.Instance.FlushFetchTimes(stations);
                Trace.TraceInformation("Fetching lasted {0}, next in {1}", sw.Elapsed, SpanToNextRegularFetch);
            }
        }
        public void MakeForecastFetching(IEnumerable<Station> stations, IEnumerable<ForecastDataSource> sources)
        {
            if (!IsFetchingGoes)
            {
                OnFetchingStarted();
                failedCases = new ConcurrentBag<Tuple<ForecastDataSource, Station>>();
                fetchingsSucceed = 0;
                fetchingsFailed = 0;

                Stopwatch sw = new Stopwatch();
                sw.Restart();
                StartFetchingTasks(stations, sources);
                sw.Stop();

                OnFetchingEnded(fetchingsSucceed, fetchingsFailed, sw.Elapsed, false);

                Trace.TraceInformation("Fetching lasted {0}, next in {1}", sw.Elapsed, SpanToNextRegularFetch);
            }
        }

        private void StartRegularFetchingTasks(IEnumerable<Station> stations)
        {
            int fSourcesNumber = SourcesDirector.Instance.ForecastDataSources.Count();
            int rSourcesNumber = SourcesDirector.Instance.RealDataSources.Count();

            Task[] tasks = new Task[fSourcesNumber + rSourcesNumber];

            for (int i = 0; i < fSourcesNumber; i++)
            {
                tasks[i] = Task.Factory.StartNew((index) =>
                {
                    int k = (int)index;
                    foreach (var station in stations)
                    {
                        FetchNewForecasts(station, SourcesDirector.Instance.ForecastDataSources.ElementAt(k));
                    }
                }, i);
            }
            for (int i = fSourcesNumber; i < fSourcesNumber + rSourcesNumber; i++)
            {
                tasks[i] = Task.Factory.StartNew((index) =>
                {
                    int k = (int)index;
                    foreach (var station in stations)
                    {
                        FetchRealData(station, SourcesDirector.Instance.RealDataSources.ElementAt(k));
                    }
                }, i - fSourcesNumber);
            }
            Task.WaitAll(tasks);
        }
        private void StartFetchingTasks(IEnumerable<Station> stations, IEnumerable<ForecastDataSource> sources)
        {
            var _sources = new List<ForecastDataSource>(sources);

            Task[] tasks = new Task[_sources.Count];

            for (int i = 0; i < _sources.Count; i++)
            {
                tasks[i] = Task.Factory.StartNew((index) =>
                {
                    int k = (int)index;
                    foreach (var station in stations)
                    {
                        FetchNewForecasts(station, _sources[k]);
                    }
                }, i);
            }
            Task.WaitAll(tasks);
        }
        private void OnFetchingFailed(Station station, IAbstractDataSource source, string message)
        {
            EventHandler<FetchingFailedEventArgs> handler = this.FetchingFailed;
            if (handler != null)
            {
                handler(this, new FetchingFailedEventArgs(station, source, message));
            }
            Interlocked.Increment(ref fetchingsFailed);
        }

        private void OnFetchingSucceeded(Station station, IAbstractDataSource source, DateTime fetchedTime)
        {
            EventHandler<FetchingSucceededEventArgs> handler = this.FetchingSucceeded;
            if (handler != null)
            {
                handler(this, new FetchingSucceededEventArgs(station, source, fetchedTime));
            }
            Interlocked.Increment(ref fetchingsSucceed);
        }

        private void OnFetchingStarted()
        {
            IsFetchingGoes = true;
            EventHandler handler = this.FetchingStarted;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void OnFetchingEnded(int fetchedCount, int notFetchedCount, TimeSpan duration, bool isRegular)
        {
            EventHandler<FetchingEndedEventArgs> handler = this.FetchingEnded;
            if (handler != null)
            {
                handler(this, new FetchingEndedEventArgs(fetchedCount, notFetchedCount, duration, isRegular));
            }
            IsFetchingGoes = false;
        }

        protected Fetcher()
        {
            try
            {
                _checkingPeriod = TimeSpan.FromMinutes(double.Parse(ConfigurationManager.AppSettings["chekingPeriodMinutes"]));
            }
            catch { }
            _nextCheckingAfter = _checkingPeriod;
        }

        #region Singleton implementation

        private static readonly Lazy<Fetcher> lazyInstance = new Lazy<Fetcher>(() => new Fetcher());

        public static Fetcher Instance
        {
            get
            {
                return lazyInstance.Value;
            }
        }

        #endregion // Singleton implementation}
    }
}
