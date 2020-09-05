using System;
using System.Globalization;

namespace Weatherlog.Models.Parameters
{
    static class PrecipitationAmountConverter
    {
        const int precisionMultiplier = 100;

        public static double ToDouble(int value)
        {
            return (double)value / precisionMultiplier;
        }
        public static string ToString(int value)
        {
            double doubleValue = ToDouble(value);
            if ((int)doubleValue == doubleValue)
            {
                return ((int)doubleValue).ToString();
            }
            else
            {
                return doubleValue.ToString("F2");
            }
        }
        public static int ToValue(string str)
        {
            double doubleValue = double.Parse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            return (int)Math.Round(doubleValue * precisionMultiplier);
        }
        public static int ToValue(double value)
        {
            return (int)Math.Round(value * precisionMultiplier);
        }
    }
}
