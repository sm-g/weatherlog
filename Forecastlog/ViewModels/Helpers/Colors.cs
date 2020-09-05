using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Weatherlog.Models.Sources;

namespace Weatherlog.ViewModels
{
    public static class ColorsExtensions
    {
        static Dictionary<string, OxyColor> colorsBySource;
        static ColorsExtensions()
        {
            GeneratePalette();
        }

        public static OxyColor OxyColor(this IAbstractDataSource source)
        {
            OxyColor res;
            if (colorsBySource.TryGetValue(source.Id, out res))
                return res;
            return OxyPlot.OxyColor.FromRgb(1, 1, 1);
        }
        public static string ColorById(string sourceId)
        {
            OxyColor res;
            if (colorsBySource.TryGetValue(sourceId, out res))
                return res.ToString();
            return System.Windows.Media.Colors.Black.ToString();
        }

        private static void GeneratePalette()
        {
            int fSourcesCount = SourcesDirector.Instance.ForecastDataSources.Count();
            int rSourcesCount = SourcesDirector.Instance.RealDataSources.Count();
            colorsBySource = new Dictionary<string, OxyColor>();

            OxyPalette palette = OxyPalettes.Jet(fSourcesCount);
            for (int i = 0; i < fSourcesCount; i++)
            {
                colorsBySource.Add(SourcesDirector.Instance.ForecastDataSources.ElementAt(i).Id, palette.Colors[i]);
            }
            palette = OxyPalettes.Gray(rSourcesCount + 1);
            for (int i = 0; i < rSourcesCount; i++)
            {
                colorsBySource.Add(SourcesDirector.Instance.RealDataSources.ElementAt(i).Id, palette.Colors[i]);
            }
        }
    }
}
