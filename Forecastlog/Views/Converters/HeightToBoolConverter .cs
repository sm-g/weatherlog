using System;
using System.Windows.Data;

namespace Weatherlog.Views
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class HeightToBoolConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value < (double)parameter;
        }

    }
}
