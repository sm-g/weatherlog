using System;
using System.Diagnostics;

namespace Weatherlog.Models.Parameters
{
    static class PercentParameterConverter
    {
        const int MAX_VALUE = 100;

        public static int Normalize(int value, int maxPossibleValue)
        {
            Debug.Assert(maxPossibleValue > 0);
            return value * MAX_VALUE / maxPossibleValue;
        }
        public static int Normalize(double value)
        {
            return (int)Math.Round(value * MAX_VALUE);
        }
    }

}
