using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Weatherlog.Computing;

namespace Weatherlog.ViewModels
{
    public class ForecastViewerViewModel : BaseViewModel
    {
        ICommand _loadSelectedForecastsCommand;
        GivenForecastTimeKind _givenTimeKind;
        DateTime _selectedDate;
        string _selectedTime;
        SourcesParametersPickerViewModel _sourceParamPicker = new SourcesParametersPickerViewModel();
        ObservableCollection<PlotViewModel> _plots = new ObservableCollection<PlotViewModel>();

        Settings _settings;
        static string allDayString = "0–23";

        #region ViewModelBase Implementation

        public override string Name { get { return "Просмотр прогнозов"; } }

        #endregion

        public DateTime SelectedDate
        {
            get
            {
                return _selectedDate;
            }
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(() => SelectedDate);
                }
            }
        }
        public string SelectedTime
        {
            get
            {
                return _selectedTime;
            }
            set
            {
                if (_selectedTime != value)
                {
                    _selectedTime = value;
                    OnPropertyChanged(() => SelectedTime);
                }
            }
        }

        public SourcesParametersPickerViewModel SourceParamPicker
        {
            get
            {
                return _sourceParamPicker;
            }
        }

        public GivenForecastTimeKind GivenTimeKind
        {
            get
            {
                return _givenTimeKind;
            }
            set
            {
                if (_givenTimeKind != value)
                {
                    _givenTimeKind = value;
                    OnPropertyChanged(() => GivenTimeKind);
                }
            }
        }
        public ObservableCollection<string> Times { get; private set; }

        public ObservableCollection<PlotViewModel> Plots { get { return _plots; } }

        public ICommand LoadSelectedForecastsCommand
        {
            get
            {
                if (_loadSelectedForecastsCommand == null)
                {
                    _loadSelectedForecastsCommand = new RelayCommand(LoadSelectedForecasts);
                }

                return _loadSelectedForecastsCommand;
            }
        }

        public ForecastViewerViewModel(Settings settings)
        {
            SetUpTimes();

            _settings = settings;
        }

        private void SetUpTimes()
        {
            GivenTimeKind = GivenForecastTimeKind.Created;
            Times = new ObservableCollection<string>();
            Times.Add(allDayString);
            foreach (var item in Enumerable.Range(0, 24).Select(x => x.ToString()))
            {
                Times.Add(item);
            }
            SelectedTime = allDayString;
#if DEBUG
            SelectedDate = new DateTime(2013, 06, 07);
#else
            SelectedDate = DateTime.Now;
#endif
        }

        private void LoadSelectedForecasts()
        {
            try
            {
                var collection = WeatherDataSelecter.GetGroupedByParameterForecastPlots(new ForecastViewerRequest(
                    _settings.CurrentStation,
                    SelectedDate.AddHours(SelectedTime == allDayString ? 0 : int.Parse(SelectedTime)),
                    GivenTimeKind,
                    SelectedTime == allDayString ? RequestTimeDeterminateness.AllDay : RequestTimeDeterminateness.DateAndHour,
                    SourceParamPicker.CheckedSources,
                    SourceParamPicker.CheckedParameters.Select(p => p.ShortTypeName)));

                foreach (var plot in Plots)
                {
                    plot.Dispose();
                }
                Plots.Clear();
                foreach (var parameter in collection.Keys)
                {
                    var plot = new PlotViewModel(collection[parameter]);
                    Plots.Add(plot);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }
    }
}
