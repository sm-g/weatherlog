using System;
using System.Windows.Data;

namespace Weatherlog.Views
{
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBoolConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
