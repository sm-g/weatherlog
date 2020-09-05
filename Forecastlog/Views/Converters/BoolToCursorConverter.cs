using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Weatherlog.Views
{
    [ValueConversion(typeof(bool), typeof(Cursor))]
    public class BoolToCursorConverter : BaseConverter, IValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isWait = (bool)value;
            if (isWait)
            {
                return Cursors.AppStarting;
            }
            else
            {
                return Cursors.Arrow;
            }
        }
    }

}
