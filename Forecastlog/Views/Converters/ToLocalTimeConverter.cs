using System;
using System.Globalization;
using System.Windows.Data;

namespace Weatherlog.Views
{
    [ValueConversion(typeof(DateTime), typeof(DateTime))]
    public class ToLocalTimeConverter : BaseConverter, IValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToLocalTime();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToUniversalTime();
        }
    }

}
