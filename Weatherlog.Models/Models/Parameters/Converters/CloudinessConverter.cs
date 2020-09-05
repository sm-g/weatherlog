using System.Collections.Generic;
using System.Linq;

namespace Weatherlog.Models.Parameters
{
    static class CloudinessConverter
    {
        const int overcast = 100;
        const int underOvercast = 90;
        const int broken = 75;
        const int scattered = 50;
        const int few = 20;
        static Dictionary<string, int> cloudinessByCode;

        static CloudinessConverter()
        {
            cloudinessByCode = new Dictionary<string, int>(8);
            // not clouds
            cloudinessByCode.Add("OVX", 0);
            // clear sky
            cloudinessByCode.Add("SKC", 0);
            cloudinessByCode.Add("CLR", 0);
            cloudinessByCode.Add("CAVOK", 0);
            // clouds percentage (approx from octets)
            cloudinessByCode.Add("FEW", few);
            cloudinessByCode.Add("SCT", scattered);
            cloudinessByCode.Add("BKN", broken);
            cloudinessByCode.Add("OVC", overcast);
        }

        public static int AggregateMetarDescrition(IList<CloudMetarDescription> values)
        {
            int valuesCount = values.Count();
            if (valuesCount == 0)
            {
                return 0;
            }

            int[] clouds = new int[valuesCount];

            for (int i = 0; i < valuesCount; i++)
            {
                cloudinessByCode.TryGetValue(values[i].descriptionAbbr, out clouds[i]);
            }
            if (valuesCount == 1)
            {
                return clouds[0];
            }

            int maxCloud = clouds.Max();
            switch (maxCloud)
            {
                case overcast:
                    return overcast;
                case broken:
                    return underOvercast;
                default:
                    return maxCloud;
            }
        }
    }

}
