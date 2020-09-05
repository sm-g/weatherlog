using System;
using System.Diagnostics;

namespace Weatherlog.Models.Parameters
{
    static class HumidityConverter
    {
        static int[,] humidities;
        static char[] delimeters = new char[] { ';' };
        static int maxTemp;

        static HumidityConverter()
        {
            // dp2rh - table of relative humidity, fisrt column - temperatures (from max to min), first row - temperature minus due point
            var rows = Properties.Resources.dp2rh.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var humRowsCount = rows.Length - 1;
            var humColumnsCount = rows[0].Split(delimeters, StringSplitOptions.None).Length - 1;
            humidities = new int[humRowsCount, humColumnsCount];

            try
            {
                for (int i = 0; i < humRowsCount; i++)
                {
                    var elements = rows[i + 1].Split(delimeters, StringSplitOptions.RemoveEmptyEntries);
                    if (i == 0)
                    {
                        maxTemp = int.Parse(elements[0]);
                    }
                    for (int k = 1; k < elements.Length; k++)
                        humidities[i, k - 1] = int.Parse(elements[k]);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Parsing dp2rh resource: {0}", e.Message);
            }
        }

        public static int FromDuePoint(int temp, int duepoint)
        {
            return humidities[maxTemp - temp, temp - duepoint];
        }
    }
}
