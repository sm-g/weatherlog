using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Weatherlog.Models;
using Weatherlog.Models.Parameters;
using Weatherlog.Models.Sources;

namespace Weatherlog.ViewModels
{
    public class SourcesViewModel : BaseViewModel
    {
        ObservableCollection<SourceWithStationBasedResults> _sources = new ObservableCollection<SourceWithStationBasedResults>();
        readonly object _sourcesLock = new Object();
        Settings _settings;

        public override string Name { get { return "Источники данных"; } }

        public class SourceWithStationBasedResults : IDisposable
        {
            internal readonly IAbstractDataSource source;
            EventHandler _timerTickHandler;
            static DispatcherTimer refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1),
                IsEnabled = true
            };

            public string Id { get { return source.Name; } }
            public string Name { get { return source.Name; } }
            public string ApiLink { get { return source.ApiDescriptionUri; } }
            public DateTime LastSuccessfullFetching { get { return source.LastSuccessfullFetching; } }
            public TimeSpan UpdatePeriod { get { return source.UpdatingDataPeriod; } }
            public DateTime LastStationFetchTime { get; private set; }
            public bool IsNewDataForStationAvailable { get; private set; }
            public string Parameters
            {
                get
                {
                    return string.Concat(source.Parameters.Select(p =>
                        ParametersFactory.GetName(p) + Environment.NewLine));
                }
            }

            public SourceWithStationBasedResults(IAbstractDataSource source, Station station)
            {
                this.source = source;

                LastStationFetchTime = source.GetLastFetchTime(station);
                IsNewDataForStationAvailable = source.IsNewDataReady(station);

                _timerTickHandler = (s, e) =>
                {
                    IsNewDataForStationAvailable = source.IsNewDataReady(station);
                };

                refreshTimer.Tick += _timerTickHandler;
            }
            public void Dispose()
            {
                refreshTimer.Tick -= _timerTickHandler;
            }
        }

        public string CurrentStation
        {
            get { return _settings.CurrentStation.Name; }
        }

        public ObservableCollection<SourceWithStationBasedResults> Sources
        {
            get { return _sources; }
            private set
            {
                if (_sources != value)
                {
                    _sources = value;
                    OnPropertyChanged(() => Sources);
                }
            }
        }

        private SourceWithStationBasedResults _selected;
        public SourceWithStationBasedResults SelectedSource
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged(() => SelectedSource);
                }
            }
        }
        public RelayCommand FetchForecastsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Fetcher.Instance.MakeForecastFetching(
                        StationsDirector.Instance.Stations,
                        new[] { SelectedSource.source as ForecastDataSource });
                }, () =>
                    !Fetcher.Instance.IsFetchingGoes &&
                    SelectedSource.source is ForecastDataSource);
            }
        }

        public SourcesViewModel(Settings settings)
        {
            _settings = settings;
            SubscribeToEvents();
        }


        private void SubscribeToEvents()
        {
            _settings.CurrentStationChanged += _settings_CurrentStationChanged;
            foreach (var source in SourcesDirector.Instance.AllDataSources)
            {
                source.LastFetchTimeChanged += source_LastFetchTimeChanged;
            }
        }

        private void ConstructSourcesInfoForStation(IEnumerable<IAbstractDataSource> newSources, Station station)
        {
            lock (_sourcesLock)
            {
                if (newSources.Count() == 1)
                {
                    var source = newSources.First();
                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var oldSource = Sources.First(s => s.Id == source.Id);
                        Sources.Remove(oldSource);
                        oldSource.Dispose();
                        Sources.Add(new SourceWithStationBasedResults(source, station));
                    }));
                }
                else
                {
                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (var s in Sources)
                        {
                            s.Dispose();
                        }
                        Sources = new ObservableCollection<SourceWithStationBasedResults>(newSources.Select(
                            source => new SourceWithStationBasedResults(source, station)));
                    }));
                }
            }
        }

        void source_LastFetchTimeChanged(object sender, LastFetchTimeChangedEventArgs e)
        {
            if (e.station == _settings.CurrentStation)
                ConstructSourcesInfoForStation(new List<IAbstractDataSource>() { e.source }, _settings.CurrentStation);
        }

        private void _settings_CurrentStationChanged(object sender, EventArgs e)
        {
            ConstructSourcesInfoForStation(SourcesDirector.Instance.AllDataSources, _settings.CurrentStation);
            OnPropertyChanged(() => CurrentStation);
        }

    }
}
