using System;
using System.Collections.Generic;

namespace Weatherlog.Models
{
    public class Station
    {
        string _name;
        int? _utcOffset;
        float? _latitude;
        float? _longitude;
        string _icao;
        Dictionary<string, string> _sourceQueryMap = new Dictionary<string, string>();

        public string Id
        {
            get
            {
                return Name.ToLower();
            }
        }

        public Dictionary<string, string> SourceQueryMap
        {
            get { return _sourceQueryMap; }
        }

        public string Icao
        {
            get { return _icao; }
            set
            {
                if (value.Length == 4)
                {
                    _icao = value.ToUpper();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value", "ICAO code length is 4.");
                }
            }
        }

        public int UtcOffset
        {
            get { return _utcOffset.Value; }
            set
            {
                if (Math.Abs(value) <= 14)
                {
                    _utcOffset = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        public float Latitude
        {
            get { return _latitude.Value; }
            set { _latitude = value; }
        }

        public float Longitude
        {
            get { return _longitude.Value; }
            set { _longitude = value; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value.Length > 1)
                { _name = value; }
                else
                { throw new ArgumentOutOfRangeException("value", "Name is too short."); }
            }
        }

        public bool HasLatLongCoordinates
        {
            get { return _latitude.HasValue && _longitude.HasValue; }
        }

        public bool HasIcaoCode
        {
            get { return Icao != null; }
        }

        public bool HasUtcOffset
        {
            get { return _utcOffset.HasValue; }
        }

        public Station(string name)
        {
            Name = name;
        }

        public Station(string name, float latitude, float longitude)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }

        public Station(string name, float latitude, float longitude, int offset)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            UtcOffset = offset;
        }

        public override string ToString()
        {
            return Name +
                (HasUtcOffset ? " UTC" + UtcOffset.ToString("+#;-#") : "") +
                (HasLatLongCoordinates ? " " + Latitude + ";" + Longitude : "");
        }
    }
}
