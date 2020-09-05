using System;

namespace Weatherlog.Models.Parameters
{
    static class PressureConverter
    {
        const double in1Hpa = 0.75;
        const double in1Inhg = 25.4;

        public static int HpaToMmhg(double hpaValue)
        {
            return (int)Math.Round(hpaValue * in1Hpa);
        }

        internal static int HpaToInhg(double inhgValue)
        {
            return (int)Math.Round(inhgValue * in1Inhg);
        }
    }
}
