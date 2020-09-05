using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Weatherlog.Models;
using Weatherlog.Models.Sources;

namespace Weatherlog.Data
{
    public static class ForecastsDatabase
    {
        const string filesExtension = ".txt";
        static readonly ConcurrentDictionary<string, byte[]> cache = new ConcurrentDictionary<string, byte[]>();
        static readonly ConcurrentDictionary<string, bool> isCacheActual = new ConcurrentDictionary<string, bool>();
        static readonly ConcurrentDictionary<string, object> ioLocks = new ConcurrentDictionary<string, object>();
        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None
        };

        public static event EventHandler<SavingFailedEventArgs> SavingFailed = delegate { };
        public static event EventHandler<LoadingFailedEventArgs> LoadingFailed = delegate { };

        public static void Save(Weather freshWeather)
        {
            string dataToWrite = String.Concat(freshWeather.Forecasts.Select(f => GetWrittenChunk(f)));
            string path = GetSourceFilename(freshWeather.Source, freshWeather.Station);

            try
            {
                CreateDataDirIfNeed(freshWeather.Station);
                lock (GetIoLock(path))
                {
                    File.AppendAllText(path, dataToWrite);
                    isCacheActual[path] = false;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Saving forecast {0}: {1}", path, e.Message);
                OnSavingFailed(freshWeather.Station, freshWeather.Source, e.Message);
            }
        }

        public static IEnumerable<Forecast> Load(Station station, IAbstractDataSource source, IEnumerable<DateTime> validTimeDates)
        {
            IEnumerable<Forecast> result = new List<Forecast>();

            if (IsSourceDatabaseExists(source, station))
            {
                List<string> patterns = validTimeDates.Select(dt => GetChunkStartMarker(dt)).ToList();

                MemoryStream ms = null;
                string path = GetSourceFilename(source, station);
                try
                {
                    ActualizeCache(path);
                    ms = new MemoryStream(cache[path]);
                    using (var sr = new StreamReader(ms))
                    {
                        result = (ReadAllChunksOfPatterns(sr, patterns));
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("Loading forecast {0}: {1}", path, e.Message);
                    OnLoadingFailed(station, source, validTimeDates, e.Message);
                }
                finally
                {
                    if (ms != null)
                        ms.Dispose();
                }
            }

            return result;
        }

        private static void ActualizeCache(string filename)
        {
            lock (GetIoLock(filename))
            {
                if (!isCacheActual.ContainsKey(filename) || !isCacheActual[filename])
                {
                    cache[filename] = File.ReadAllBytes(filename);
                    isCacheActual[filename] = true;
                }
            }
        }

        private static object GetIoLock(string filename)
        {
            if (!ioLocks.ContainsKey(filename))
            {
                ioLocks[filename] = new Object();
            }
            return ioLocks[filename];
        }

        #region Chunks

        private static IEnumerable<Forecast> ReadAllChunksOfPatterns(StreamReader reader, IEnumerable<string> chunkHeaders)
        {
            List<Forecast> result = new List<Forecast>();
            string line;
            string json;
            line = reader.ReadLine();
            while (line != null)
            {
                if (chunkHeaders.Contains(line))
                {
                    json = "";
                    line = reader.ReadLine();
                    while (line != null && line != GetChunkEndMarker())
                    {
                        json += line;
                        line = reader.ReadLine();
                    }
                    result.Add(JsonConvert.DeserializeObject<Forecast>(json, jsonSerializerSettings));
                }

                line = reader.ReadLine();
            }
            return result;
        }

        private static string GetWrittenChunk(Forecast forecast)
        {
            return String.Concat(
                GetChunkStartMarker(forecast.ValidTime), "\r\n",
                JsonConvert.SerializeObject(forecast, jsonSerializerSettings), "\r\n",
                GetChunkEndMarker(), "\r\n");
        }

        private static string GetChunkStartMarker(DateTime validTime)
        {
            return validTime.Date.ToString("dd.MM.yyyy") + " (";
        }

        private static string GetChunkEndMarker()
        {
            return ")";
        }

        #endregion

        #region File System

        private static bool IsSourceDatabaseExists(IAbstractDataSource source, Station station)
        {
            return new FileInfo(GetSourceFilename(source, station)).Exists;
        }

        private static string GetSourceFilename(IAbstractDataSource source, Station station)
        {
            return StationsDatabase.GetStationDir(station) + source.Id + filesExtension;
        }

        private static void CreateDataDirIfNeed(Station station)
        {
            Directory.CreateDirectory(StationsDatabase.GetStationDir(station));
        }

        #endregion

        private static void OnSavingFailed(Station station, IAbstractDataSource source, string message)
        {
            SavingFailed(null, new SavingFailedEventArgs(station, source, message));
        }
        private static void OnLoadingFailed(Station station, IAbstractDataSource source, IEnumerable<DateTime> dates, string message)
        {
            LoadingFailed(null, new LoadingFailedEventArgs(station, source, dates, message));
        }
    }
}
