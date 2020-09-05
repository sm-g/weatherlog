
namespace Weatherlog.Models.Parameters
{
    static class PrecipitationKindConverter
    {
        static string[] rains = new string[] { "RA", "DZ" };
        static string[] snows = new string[] { "SN", "SG", "IC", "PL", "GR", "GS" };
        const int noPrecip = 0;
        const int rain = 1;
        const int snow = 2;

        internal const int min = noPrecip;
        internal const int max = noPrecip + rain + snow;

        public static int MetarToValue(string wx)
        {
            int result = noPrecip;
            foreach (var item in rains)
            {
                if (wx.Contains(item))
                {
                    result += rain;
                    break;
                }
            }
            foreach (var item in snows)
            {
                if (wx.Contains(item))
                {
                    result += snow;
                    break;
                }
            }
            return result;
        }

        public static int GismeteoCodeToValue(string code)
        {
            // 4 - дождь, 5 - ливень, 6,7 – снег, 8 - гроза, 9 - нет данных, 10 - без осадков
            int codeValue = int.Parse(code);
            switch (codeValue)
            {
                case 4:
                case 5:
                    return rain;
                case 6:
                case 7:
                    return snow;
                default:
                    return noPrecip;
            }
        }
    }
}
