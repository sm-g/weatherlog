using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Weatherlog.Computing;


namespace Weatherlog.ViewModels
{
    public class AccuracyViewModel : BaseViewModel
    {
        ICommand _fetchSelectedForecastsCommand;
        ObservableCollection<PlotViewModel> _plots = new ObservableCollection<PlotViewModel>();
        ObservableCollection<AccuracyPlotViewModel> _accuracyPlots = new ObservableCollection<AccuracyPlotViewModel>();

        DateTime _selectedDate;
        AccuracyComparison _selectedComparisonMode;
        SourcesParametersPickerViewModel _sourceParamPicker = new SourcesParametersPickerViewModel();

        Settings _settings;

        public override string Name { get { return "Точность прогнозов"; } }

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
        public AccuracyComparison SelectedComparisonMode
        {
            get
            {
                return _selectedComparisonMode;
            }
            set
            {
                if (_selectedComparisonMode != value)
                {
                    _selectedComparisonMode = value;
                    OnPropertyChanged(() => SelectedComparisonMode);
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

        public ObservableCollection<AccuracyComparison> ComparisonModes { get; private set; }

        public ObservableCollection<PlotViewModel> Plots { get { return _plots; } }
        public ObservableCollection<AccuracyPlotViewModel> AccuracyPlots { get { return _accuracyPlots; } }

        public ICommand LoadSelectedCommand
        {
            get
            {
                if (_fetchSelectedForecastsCommand == null)
                {
                    _fetchSelectedForecastsCommand = new RelayCommand(LoadSelected);
                }

                return _fetchSelectedForecastsCommand;
            }
        }

        public AccuracyViewModel(Settings settings)
        {
            _settings = settings;
            SetUpComparisonModes();
        }

        private void SetUpComparisonModes()
        {
            ComparisonModes = new ObservableCollection<AccuracyComparison>();
            foreach (AccuracyComparison item in Enum.GetValues(typeof(AccuracyComparison)))
            {
                ComparisonModes.Add(item);
            }

            SelectedComparisonMode = AccuracyComparison.HalfDay;
#if DEBUG
            SelectedDate = new DateTime(2013, 06, 07);
#else
            SelectedDate = DateTime.Now;
#endif
        }

        private void LoadSelected()
        {
            try
            {
                var plotDatas = WeatherDataSelecter.GetGroupedByParameterAccuracyPlots(new AccuracyRequest(
                    _settings.CurrentStation,
                    SelectedDate,
                    SelectedComparisonMode,
                    SourceParamPicker.CheckedSources,
                    SourceParamPicker.CheckedParameters.Select(p => p.ShortTypeName)));

                foreach (var plot in Plots)
                {
                    plot.Dispose();
                }
                Plots.Clear();

                AccuracyPlots.Clear();

                foreach (var parameter in plotDatas.Keys)
                {
                    var plot = new PlotViewModel(plotDatas[parameter]);
                    var acPlot = new AccuracyPlotViewModel(AccuracyMeter.CalcAccuracy(plotDatas[parameter], StatisticMethods.All));
                    Plots.Add(plot);
                    AccuracyPlots.Add(acPlot);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }
    }
}
