using System;
using System.Globalization;
using System.Windows.Data;
using Weatherlog.Models.Sources;
using Weatherlog.ViewModels;

namespace Weatherlog.Views
{
    [ValueConversion(typeof(string), typeof(string))]
    public class SourceToPlotColorConverter : BaseConverter, IValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ColorsExtensions.ColorById(value as string);
        }

    }

}
