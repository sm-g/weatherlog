using System.Collections.ObjectModel;
using Weatherlog.Models;
using Weatherlog.Models.Sources;

namespace Weatherlog.ViewModels
{
    public class StationsViewModel : BaseViewModel
    {
        private ObservableCollection<Station> _stations;
        private Settings _settings;

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

        private Station _selected;
        public Station SelectedStation
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
                    OnPropertyChanged(() => SelectedStation);
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
                        new[] { SelectedStation },
                        SourcesDirector.Instance.ForecastDataSources);
                }, () =>
                    !Fetcher.Instance.IsFetchingGoes);
            }
        }

        public StationsViewModel(Settings settings)
        {
            Stations = new ObservableCollection<Station>(StationsDirector.Instance.Stations);
            _settings = settings;
        }

        #region ViewModelBase Implementation

        public override string Name { get { return "Станции"; } }

        #endregion
    }
}
