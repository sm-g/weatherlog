using System;

namespace Weatherlog.Models.Parameters
{
    static class WindSpeedConverter
    {
        const double in1Kmph = 0.278;
        const double in1Knot = 0.514;

        public static int KmphToMps(double kmphValue)
        {
            return (int)Math.Round(in1Kmph * kmphValue);
        }

        public static int KnotToMps(double knotValue)
        {
            return (int)Math.Round(in1Knot * knotValue);
        }
    }
}
