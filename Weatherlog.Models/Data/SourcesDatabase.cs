using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Weatherlog.Models;
using Weatherlog.Models.Sources;

namespace Weatherlog.Data
{
    public static class SourcesDatabase
    {
        const string sourcesFilename = "sources.xml";
        const string xmlLastFetch = "lastfetchtime";
        const string xmlStation = "station";
        static readonly ConcurrentDictionary<string, object> ioLocks = new ConcurrentDictionary<string, object>();

        public static void SaveLastFetchTimes(IEnumerable<IAbstractDataSource> sources, IEnumerable<Station> stations)
        {
            foreach (var station in stations)
            {
                var doc = CreateXSourceTimes(sources, station).
                    ToXDocument("sources", new XAttribute[] { new XAttribute(xmlStation, station.Id) });

                string filePath = StationsDatabase.GetStationDir(station) + sourcesFilename;
                try
                {
                    doc.Save(filePath);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Saving {0}: {1}", filePath, e.Message);
                }
            }
        }

        public static Dictionary<string, DateTime> LoadLastFetchTimes(Station station)
        {
            var result = new Dictionary<string, DateTime>();

            string filePath = StationsDatabase.GetStationDir(station) + sourcesFilename;
            lock (GetIoLock(filePath))
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        var doc = XDocument.Load(filePath);
                        foreach (var source in doc.Root.Element(xmlLastFetch).Elements())
                        {
                            result.Add(source.Name.LocalName, DateTime.Parse(source.Value).ToUniversalTime());
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("Loading {0}: {1}", filePath, e.Message);
                    }
                }
            }
            return result;
        }

        private static XElement CreateXSourceTimes(IEnumerable<IAbstractDataSource> sources, Station station)
        {
            var xLastFetchTime = new XElement(xmlLastFetch);
            foreach (var source in sources)
            {
                xLastFetchTime.Add(new XElement(source.Id, source.GetLastFetchTime(station)));
            }

            return xLastFetchTime;
        }

        private static object GetIoLock(string filename)
        {
            if (!ioLocks.ContainsKey(filename))
            {
                ioLocks[filename] = new Object();
            }
            return ioLocks[filename];
        }
    }
}
