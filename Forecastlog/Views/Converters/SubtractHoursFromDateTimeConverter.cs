using System;
using System.Windows.Data;

namespace Weatherlog.Views
{
    [ValueConversion(typeof(DateTime), typeof(DateTime))]
    public class SubtractHoursFromDateTime : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            var datetime = (DateTime)value;
            var hours = TimeSpan.FromHours((double)parameter);
            return datetime.Subtract(hours);
        }

    }
}
