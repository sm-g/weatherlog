using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Weatherlog.Models;

namespace Weatherlog.Data
{
    public static class StationsDatabase
    {
        const string defaultFilesDir = "data\\";
        const string xmlStation = "station";
        const string xmlName = "name";
        const string xmlUtcOffset = "offset";
        const string xmlIcao = "icao";
        const string xmlLat = "lat";
        const string xmlLon = "lon";
        const string xmlQueries = "sourcequery";
        static string filesDir = defaultFilesDir;
        static string stationsFilePath = filesDir + "stations.xml";
        static CultureInfo databaseCulture = CultureInfo.InvariantCulture;

        public static event EventHandler<StationNameInvalidEventArgs> StationNameInvalid = delegate { };

        public static void Save(IEnumerable<Station> stations)
        {
            var doc = stations.Select(s => CreateXStation(s)).ToXDocument("stations");

            try
            {
                doc.Save(filesDir + stationsFilePath);
            }
            catch (Exception e)
            {
                Trace.TraceError("Saving {0}: {1}", stationsFilePath, e.Message);
            }
        }

        public static IEnumerable<Station> Load()
        {
            List<Station> stations = new List<Station>();
            List<string> existingNames = new List<string>();

            if (File.Exists(stationsFilePath))
            {
                CultureInfo curCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = databaseCulture;
                try
                {
                    var doc = XDocument.Load(stationsFilePath);
                    foreach (var element in doc.Root.Elements(xmlStation))
                    {
                        var station = ParseXStation(element);
                        if (station != null && !existingNames.Contains(station.Name))
                        {
                            stations.Add(station);
                            existingNames.Add(station.Name);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("Loading {0}: {1}", stationsFilePath, e.Message);
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = curCulture;
                }

            }

            return stations;
        }

        internal static string GetStationDir(Station station)
        {
            return filesDir + station.Id + "\\";
        }

        static StationsDatabase()
        {
            try
            {
                filesDir = ConfigurationManager.AppSettings["dataDirectory"] + "\\";
            }
            catch { }
        }

        private static XElement CreateXStation(Station station)
        {
            var xStation = new XElement(xmlStation);

            xStation.Add(new XElement(xmlName, station.Name));
            if (station.HasUtcOffset)
            {
                xStation.Add(new XElement(xmlUtcOffset, station.UtcOffset));
            }
            if (station.HasIcaoCode)
            {
                xStation.Add(new XElement(xmlIcao, station.Icao));
            }
            if (station.HasLatLongCoordinates)
            {
                xStation.Add(new XElement(xmlLat, station.Latitude.ToString(databaseCulture)),
                      new XElement(xmlLon, station.Longitude.ToString(databaseCulture)));
            }
            if (station.SourceQueryMap.Count > 0)
            {
                var q = new XElement(xmlQueries);
                foreach (var query in station.SourceQueryMap)
                {
                    q.Add(new XElement(query.Key, query.Value));
                }
                xStation.Add(q);
            }
            return xStation;
        }

        private static Station ParseXStation(XElement xStation)
        {
            var xname = xStation.Element(xmlName);
            Station station = null;
            if (xname != null)
            {
                var name = xname.Value;
                if (IsValidName(name))
                {
                    var xoffset = xStation.Element(xmlUtcOffset);
                    var xicao = xStation.Element(xmlIcao);
                    var xlat = xStation.Element(xmlLat);
                    var xlon = xStation.Element(xmlLon);
                    var xqueries = xStation.Element(xmlQueries);

                    station = new Station(name);

                    int offsetResult;
                    if (xoffset != null && int.TryParse(xoffset.Value, out offsetResult))
                    {
                        station.UtcOffset = offsetResult;
                    }

                    if (xicao != null && xicao.Value.Length == 4)
                        station.Icao = xicao.Value;

                    float result;

                    if (xlat != null && float.TryParse(xlat.Value, out result))
                    {
                        station.Latitude = result;
                    }

                    if (xlon != null && float.TryParse(xlon.Value, out result))
                    {
                        station.Longitude = result;
                    }

                    if (xqueries != null)
                    {
                        foreach (var query in xqueries.Elements())
                        {
                            if (query.Value.Length > 0)
                            {
                                station.SourceQueryMap.Add(query.Name.ToString(), query.Value);
                            }
                        }
                    }

                }
                else
                {
                    OnStationNameInvalid(name);
                }
            }
            return station;
        }

        private static bool IsValidName(string name)
        {
            if (name.Length == 0)
                return false;

            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char ch in invalidChars)
            {
                if (name.Contains(ch))
                {
                    return false;
                }
            }

            return true;
        }

        private static void OnStationNameInvalid(string name)
        {
            StationNameInvalid(null, new StationNameInvalidEventArgs(name));
        }
    }
}
