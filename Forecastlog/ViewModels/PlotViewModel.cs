using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using Weatherlog.Computing;
using Weatherlog.Models.Parameters;
using Weatherlog.Models.Sources;

namespace Weatherlog.ViewModels
{
    public class PlotViewModel : BaseViewModel, IDisposable
    {
        const string targetDateTimeFormat = "d/M/yy H:m";
        const string groupingDateTimeFormat = "d/M/yy H:mm";
        const string groupingDateFormat = "d/M/yy";
        const string timePickerDateTimeFormat = "d MMMM, H ч.";
        PlotModel _plotModel;
        double _minHeight = 230;
        EventHandler<OxyMouseEventArgs> _timePickerDownHandler;
        EventHandler<OxyMouseEventArgs> _timePickerUpHandler;
        EventHandler<OxyMouseEventArgs> _timePickerMoveHandler;
        EventHandler<AxisChangedEventArgs> _dateAxisHandler;
        static LineAnnotation timePickerAnnotation;
        static DateTimeAxis dateAxis;
        static List<PlotModel> plotsGroup;
        static int currentGroup;

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

        public override string Name { get { return "График"; } }

        #endregion

       
        public PlotViewModel(PlotData data)
        {
            bool isNewGroup = false;
            if (currentGroup != data.PlotGroupKey)
            {
                isNewGroup = true;
                currentGroup = data.PlotGroupKey;
                plotsGroup = new List<PlotModel>();
            }

            SetUpTimePicker(isNewGroup);

            GivenForecastTimeKind groupingTimeKind;
            if (data.Values.Count != 0 && data.Values.First().Value.Count() != 0)
            {
                groupingTimeKind = data.Values.First().Value.First().GroupingTimeKind;
            }
            else
            {
                groupingTimeKind = GivenForecastTimeKind.Created;
            }

            SetUpDateAxis(isNewGroup, GetRelationPreposition(groupingTimeKind, false));

            SetUpPlot(data.ParameterType);
            plotsGroup.Add(PlotModel);

            foreach (var source in data.Values.Keys.OrderByDescending(s => s is ForecastDataSource))
            {
                foreach (var series in data.Values[source])
                {
                    AddDataSeries(series, source, source.OxyColor());
                }
            }
        }

        public void Dispose()
        {
            timePickerAnnotation.MouseDown -= _timePickerDownHandler;
            timePickerAnnotation.MouseMove -= _timePickerMoveHandler;
            timePickerAnnotation.MouseUp -= _timePickerUpHandler;
            dateAxis.AxisChanged -= _dateAxisHandler;
        }

        private void AddDataSeries(ParameterTimeSeries data, IAbstractDataSource source, OxyColor color)
        {
            string timeRelation = GetRelationPreposition(data.GroupingTimeKind, true);
            var lineSerie = new LineSeries
            {
                StrokeThickness = 2,
                Color = color,
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = color,
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = source.Name + "\r\n{0}\r\n{1} {2:" + targetDateTimeFormat + "}\r\n{3}: {4} " + ParametersFactory.GetUnit(data.Type),
                Title = string.Format("{0} {1} {2:" + (source is ForecastDataSource ? groupingDateTimeFormat : groupingDateFormat) + "}", data.Length, timeRelation, data.GroupingTime),
                Smooth = false,
            };
            for (int i = 0; i < data.Length; i++)
            {
                lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(data.TargetTimes[i]), data.RealValues[i]));
            }
            PlotModel.Series.Add(lineSerie);
        }

        private static string GetRelationPreposition(GivenForecastTimeKind groupingTimeKind, bool forSerie)
        {
            if (groupingTimeKind == GivenForecastTimeKind.Valid && forSerie
                ||
                groupingTimeKind != GivenForecastTimeKind.Valid && !forSerie)
            {
                return "на";
            }
            else
            {
                return "от";
            }
        }

        private void SetUpPlot(string valType)
        {
            PlotModel = new OxyPlot.PlotModel
            {
                Title = ParametersFactory.GetName(valType),
                TitlePadding = 0,
                Padding = new OxyThickness(1, 10, 1, 0),
                PlotAreaBorderThickness = 1,
                PlotAreaBorderColor = OxyColor.FromAColor(64, OxyColors.Black),
                LegendOrientation = LegendOrientation.Vertical,
                LegendBackground = OxyColor.FromAColor(128, OxyColors.White),
                LegendItemSpacing = 0,
                LegendColumnSpacing = 0,
                LegendPlacement = LegendPlacement.Outside,
            };

            var valueAxis = new LinearAxis(AxisPosition.Left)
            {
                AbsoluteMaximum = ParametersFactory.GetMaxValue(valType),
                AbsoluteMinimum = ParametersFactory.GetMinValue(valType),
                MinimumPadding = 0.05,
                MaximumPadding = 0.05,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = ParametersFactory.GetName(valType),
            };

            string unit = ParametersFactory.GetUnit(valType);
            if (unit.Length > 0)
            {
                valueAxis.Unit = unit;
                valueAxis.TitleFormatString = "{0}, {1}";
            }

            PlotModel.Annotations.Add(timePickerAnnotation);
            PlotModel.Axes.Add(dateAxis);
            PlotModel.Axes.Add(valueAxis);
        }

        private void SetUpDateAxis(bool reset, string relation)
        {
            if (reset)
            {
                dateAxis = new DateTimeAxis()
                {
                    MinimumRange = 0.25, // 6 hours
                    MinimumPadding = 0.05,
                    MaximumPadding = 0.05,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    StringFormat = targetDateTimeFormat,
                    IntervalLength = 80,
                    Title = "Данные " + relation,
                };
                _dateAxisHandler = (s, e) =>
                {
                    foreach (var model in plotsGroup)
                    {
                        model.RefreshPlot(false);
                    }
                };

                dateAxis.AxisChanged += _dateAxisHandler;
            }
        }

        private void SetUpTimePicker(bool reset)
        {
            if (reset)
            {
                timePickerAnnotation = new LineAnnotation();
                timePickerAnnotation.Type = LineAnnotationType.Vertical;
                timePickerAnnotation.Selectable = true;
                timePickerAnnotation.TextOrientation = AnnotationTextOrientation.Horizontal;
                timePickerAnnotation.Layer = AnnotationLayer.AboveSeries;
                timePickerAnnotation.TextMargin = 20;
                timePickerAnnotation.TextPadding = 10;
                timePickerAnnotation.X = DateTimeAxis.ToDouble(DateTime.Now);
            }

            _timePickerDownHandler = (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                {
                    return;
                }
                timePickerAnnotation.StrokeThickness = 5;
                timePickerAnnotation.Text = DateTimeAxis.ToDateTime(timePickerAnnotation.X).ToString(timePickerDateTimeFormat);
                PlotModel.RefreshPlot(false);
                e.Handled = true;
            };
            _timePickerMoveHandler = (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                {
                    return;
                }
                timePickerAnnotation.X = timePickerAnnotation.InverseTransform(e.Position).X;
                timePickerAnnotation.Text = DateTimeAxis.ToDateTime(timePickerAnnotation.X).ToString(timePickerDateTimeFormat);
                PlotModel.RefreshPlot(false);
                e.Handled = true;
            };
            _timePickerUpHandler = (s, e) =>
            {
                timePickerAnnotation.StrokeThickness = 1;
                timePickerAnnotation.Text = "";
                PlotModel.RefreshPlot(false);
                e.Handled = true;
            };

            timePickerAnnotation.MouseDown += _timePickerDownHandler;
            timePickerAnnotation.MouseMove += _timePickerMoveHandler;
            timePickerAnnotation.MouseUp += _timePickerUpHandler;
        }
    
    }
}
