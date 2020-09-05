using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Weatherlog.Computing;
using Weatherlog.Data;
using Weatherlog.Models;

namespace Weatherlog.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        ICommand _changePageCommand;
        BaseViewModel _currentPageViewModel;
        BaseViewModel _prevPageViewModel;
        Collection<BaseViewModel> _pageViewModels;
        bool _isStationEditing;

        ObservableCollection<Station> _stations;
        Settings _settings = new Settings();
        Logger _logger = new Logger();
        string _fetchingMeassage;
        bool _isFetching;
        int savesFailed;
        private string prevMessage;

        #region ViewModelBase Implementation

        public override string Name { get { return "Главное окно"; } }

        #endregion

        #region Page Navigation

        public ICommand ChangePageCommand
        {
            get
            {
                if (_changePageCommand == null)
                {
                    _changePageCommand = new RelayCommand<BaseViewModel>(
                        p => ChangePageViewModel((BaseViewModel)p),
                        p => p is BaseViewModel);
                }

                return _changePageCommand;
            }
        }
        public Collection<BaseViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                    _pageViewModels = new Collection<BaseViewModel>();

                return _pageViewModels;
            }
        }
        public BaseViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    OnPropertyChanged(() => CurrentPageViewModel);
                }
            }
        }

        private void ChangePageViewModel(BaseViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
                PageViewModels.Add(viewModel);
            if (IsStationsEditMode)
            {
                IsStationsEditMode = false;
            }
            CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
        }

        #endregion // Page Navigation

        #region Page ViewModels

        public ForecastViewerViewModel ForecastViewer
        {
            get;
            private set;
        }

        public StationsViewModel StationsViewModel
        {
            get;
            private set;
        }

        public SourcesViewModel SourcesViewModel
        {
            get;
            private set;
        }

        public AccuracyViewModel AccuracyViewModel
        {
            get;
            private set;
        }

        #endregion // Page ViewModels

        #region FetchingIndicators

        public string Log
        {
            get
            {
                return _logger.AllText;
            }
            set
            {
                _logger.AddLine(value);
                OnPropertyChanged(() => Log);
            }
        }

        public bool IsFetching
        {
            get
            {
                return _isFetching;
            }
            set
            {
                if (_isFetching != value)
                {
                    _isFetching = value;
                    OnPropertyChanged(() => IsFetching);
                }
            }
        }

        public string FetchingMessage
        {
            get
            {
                return _fetchingMeassage;
            }
            set
            {
                if (_fetchingMeassage != value)
                {
                    _fetchingMeassage = value;
                    OnPropertyChanged(() => FetchingMessage);
                }
            }
        }

        #endregion

        #region Stations

        public ObservableCollection<Station> Stations
        {
            get { return _stations; }
            private set
            {
                if (_stations != value)
                {
                    _stations = value;
                    OnPropertyChanged(() => Stations);
                }
            }
        }

        public Station CurrentStation
        {
            get { return _settings.CurrentStation; }
            set
            {
                _settings.CurrentStation = value;
                OnPropertyChanged(() => CurrentStation);
            }
        }

        public bool IsStationsEditMode
        {
            get { return _isStationEditing; }
            set
            {
                if (_isStationEditing != value)
                {
                    _isStationEditing = value;
                    OnPropertyChanged(() => IsStationsEditMode);
                    SetStationsPage();
                }
            }
        }

        private void SetStationsPage()
        {
            if (IsStationsEditMode)
            {
                _prevPageViewModel = CurrentPageViewModel;
                CurrentPageViewModel = StationsViewModel;
            }
            else
            {
                CurrentPageViewModel = _prevPageViewModel;
            }
        }

        #endregion

        public MainViewModel()
        {
            Stations = new ObservableCollection<Station>(StationsDirector.Instance.Stations);

            SubcribeToInfoEvents();

            CreateViewModels();

            ChangePageViewModel(AccuracyViewModel);
            _settings.CurrentStation = Stations.Last();
        }

        private void CreateViewModels()
        {
            StationsViewModel = new StationsViewModel(_settings);
            SourcesViewModel = new SourcesViewModel(_settings);
            ForecastViewer = new ForecastViewerViewModel(_settings);
            AccuracyViewModel = new AccuracyViewModel(_settings);
            PageViewModels.Add(AccuracyViewModel);
            PageViewModels.Add(SourcesViewModel);
            PageViewModels.Add(ForecastViewer);
        }

        private void SubcribeToInfoEvents()
        {
            Fetcher.Instance.FetchingFailed += Fetcher_FetchingFailed;
            Fetcher.Instance.FetchingSucceeded += Fetcher_FetchingSucceeded;
            Fetcher.Instance.FetchingStarted += Fetcher_RegularFetchingStarted;
            Fetcher.Instance.FetchingEnded += Fetcher_RegularFetchingEnded;
            ForecastsDatabase.SavingFailed += ForecastsDatabase_SavingFailed;
            StationsDatabase.StationNameInvalid += StationsDatabase_StationNameInvalid;
            ForecastsDatabase.LoadingFailed += ForecastsDatabase_LoadingFailed;
            WeatherDataSelecter.SelectingWeatherDataFailed += WeatherDataSelecter_SelectingWeatherDataFailed;
        }

        void Fetcher_RegularFetchingEnded(object sender, FetchingEndedEventArgs e)
        {
            IsFetching = false;
            string info = String.Format("Получение прогнозов заняло {0:h\\:m\\:s}, всего получено: {1}", e.duration, e.successfullyFetched);
            if (e.notFetched > 0)
                info += String.Format(", не получено: {0}", e.notFetched);
            if (savesFailed > 0)
                info += String.Format(", не сохранено: {0}", savesFailed);
            Log = info + Environment.NewLine;

            if (e.isRegular)
            {
                FetchingMessage = "Следующее получение в " + DateTime.Now.Add(Fetcher.Instance.SpanToNextRegularFetch).ToLongTimeString();
            }
            else
                FetchingMessage = prevMessage;
        }

        void Fetcher_RegularFetchingStarted(object sender, EventArgs e)
        {
            IsFetching = true;
            savesFailed = 0;
            prevMessage = FetchingMessage;
            FetchingMessage = "Получение прогнозов...";
        }

        void StationsDatabase_StationNameInvalid(object sender, StationNameInvalidEventArgs e)
        {
            Log = String.Format("Недопустимое имя станции: {0}.", e.stationName);
        }

        void ForecastsDatabase_SavingFailed(object sender, SavingFailedEventArgs e)
        {
            savesFailed++;
            Log = String.Format("Прогноз для {0} из {1} не сохранён: {2}.", e.station.Name, e.source.Name, e.message);
        }


        void Fetcher_FetchingSucceeded(object sender, FetchingSucceededEventArgs e)
        {
            Log = String.Format("{0} Получен прогноз для {1} из {2}.", e.time.ToLongTimeString(), e.station.Name, e.source.Name);
        }

        void Fetcher_FetchingFailed(object sender, FetchingFailedEventArgs e)
        {
            Log = String.Format("Прогноз для {0} из {1} не получен: {2}.", e.station.Name, e.source.Name, e.message);
        }

        void ForecastsDatabase_LoadingFailed(object sender, LoadingFailedEventArgs e)
        {
            System.Windows.MessageBox.Show(String.Format("Ошибка при загрузке данных для {0} из {1}: {2}", e.source.Name, e.station.Name, e.message));
        }
        void WeatherDataSelecter_SelectingWeatherDataFailed(object sender, SelectingWeatherDataFailedEventArgs e)
        {
            System.Windows.MessageBox.Show(String.Format("Ошибка при выборке данных: {0}", e.message));
        }

    }
}
