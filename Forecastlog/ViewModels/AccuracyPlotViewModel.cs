using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using Weatherlog.Computing;
using Weatherlog.Models.Parameters;
using Weatherlog.Models.Sources;

namespace Weatherlog.ViewModels
{
    public class AccuracyPlotViewModel : BaseViewModel
    {
        PlotModel _plotModel;
        double _minHeight = 230;

        public double MinHeight
        {
            get { return _minHeight; }
            set { _minHeight = value; OnPropertyChanged(() => MinHeight); }
        }

        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set { _plotModel = value; OnPropertyChanged(() => PlotModel); }
        }

        #region ViewModelBase Implementation

        public override string Name { get { return "Точность"; } }

        #endregion

        public AccuracyPlotViewModel(AccuracyResult data)
        {
            SetUpPlot(data.ParameterType, data.RealSource);

            var categories = new List<StatisticMethods>();
            categories = (from source in data.Values.Keys
                          from result in data.Values[source]
                          select result.Key).Distinct().ToList();

            foreach (var source in data.Values.Keys)
            {
                var columnSerie = new ColumnSeries()
                {
                    FillColor = source.OxyColor(),
                    TrackerFormatString = source.Name + "\r\n" + ParametersFactory.GetName(data.ParameterType) + "\r\n{1}: {2:0.000}",
                };

                foreach (var results in data.Values[source])
                {
                    columnSerie.Items.Add(new ColumnItem(results.Value, categories.IndexOf(results.Key)));
                }
                PlotModel.Series.Add(columnSerie);
            }

            var categoryAxis = new CategoryAxis()
            {
                IsZoomEnabled = false,
                IsPanEnabled = false,
                GapWidth = 0.5
            };

            foreach (var item in categories)
            {
                categoryAxis.Labels.Add(item.ToString());
            }
            PlotModel.Axes.Add(categoryAxis);
        }

        private void SetUpPlot(string valType, RealDataSource realSource)
        {
            PlotModel = new OxyPlot.PlotModel
            {
                Title = ParametersFactory.GetName(valType),
                Subtitle = "данные от " + realSource.Name,
                TitlePadding = 0,
                Padding = new OxyThickness(1, 10, 1, 0),
                PlotAreaBorderThickness = 1,
                PlotAreaBorderColor = OxyColor.FromAColor(64, OxyColors.Black),
            };

            var valueAxis = new LinearAxis(AxisPosition.Left)
            {
                AbsoluteMinimum = 0,
                MinimumPadding = 0,
                MaximumPadding = 0.05,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };

            PlotModel.Axes.Add(valueAxis);
        }
    }
}
