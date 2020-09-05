using System.Diagnostics;

namespace Weatherlog.Models.Parameters
{
    static class WindDirectionConverter
    {
        internal const int MAX_VALUE = 360;
        const int precisionMultiplier = 100;

        static string[] directionCodes = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
        static int singleDirectionAngleWidth = MAX_VALUE * precisionMultiplier / directionCodes.Length;

        public static string ValueToCode(int value)
        {
            int i = (value * precisionMultiplier + singleDirectionAngleWidth / 2) / (singleDirectionAngleWidth);
            return directionCodes[i % directionCodes.Length];
        }

        public static int CodeToValue(string code)
        {
            Debug.Assert(code != null);

            string direction = code.ToUpper();
            for (int i = 0; i < directionCodes.Length; i++)
            {
                if (directionCodes[i] == direction)
                {
                    return i * singleDirectionAngleWidth / precisionMultiplier;
                }
            }
            Trace.TraceWarning("WindDirectionConverter: unknown direction '{0}' considered as 'N'", code);
            return 0;
        }

        public static int Normalize(int value, int maxPossibleValue)
        {
            Debug.Assert(maxPossibleValue > 0);
            return value * MAX_VALUE / maxPossibleValue;
        }

    }
}
