using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Common.Palettes;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System;
using System.Windows.Media;

namespace Weatherlog.Models
{
    public static class Graphic
    {
        public static ChartPlotter CreateChartPlotter()
        {
            ChartPlotter plotter = SamplePlot();

            return plotter;
        }
        private static ChartPlotter SamplePlot()
        {
            ChartPlotter plotter = new ChartPlotter();
            HorizontalDateTimeAxis dateAxis = new HorizontalDateTimeAxis();
            plotter.HorizontalAxis = dateAxis;

            // chart.Children.Add(plotter);

            int size = 15;
            Random random = new Random();
            DateTime[] dates = new DateTime[size];
            int[] values1 = new int[size];
            int[] values2 = new int[size];

            for (int i = 0; i < size; ++i)
            {
                dates[i] = DateTime.Today.AddDays(i);
                values1[i] = random.Next(0, 10);
                values2[i] = random.Next(5, 15);
            }

            var datesDataSource = new EnumerableDataSource<DateTime>(dates);
            datesDataSource.SetXMapping(x => dateAxis.ConvertToDouble(x));

            var numberOpenDataSource = new EnumerableDataSource<int>(values1);
            numberOpenDataSource.SetYMapping(y => y);

            var numberOpenDataSource2 = new EnumerableDataSource<int>(values2);
            numberOpenDataSource2.SetYMapping(y => y);

            var datesDataSource2 = new EnumerableDataSource<DateTime>(dates);
            datesDataSource2.SetXMapping(x => dateAxis.ConvertToDouble(((DateTime)x).AddDays(2)));

            CompositeDataSource compositeDataSource1 = new CompositeDataSource(datesDataSource, numberOpenDataSource);
            CompositeDataSource compositeDataSource2 = new CompositeDataSource(
                datesDataSource2,
                numberOpenDataSource2);
            LinearPalette pal = new LinearPalette(Colors.Crimson, Colors.DarkBlue);

            plotter.AddLineGraph(compositeDataSource1,
                new Pen(Brushes.Blue, 2),
                new CirclePointMarker { Size = 10.0, Fill = Brushes.Red },
                new PenDescription("1n"));
            plotter.AddLineGraph(compositeDataSource2,
                new Pen(Brushes.Cyan, 1),
                new TrianglePointMarker { Size = 5.0 },
                new PenDescription("2n"));

            return plotter;
        }

    }
}
