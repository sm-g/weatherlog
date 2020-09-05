using System;
using System.Collections.Generic;

namespace Weatherlog.Models.Parameters
{
    public class Cloudiness : AbstractParameter, IComparable
    {
        private static string name = "Облачность";
        private static bool zeroable = true;
        private static string unit = "%";

        public const string TypeName = "clouds";
        public const int MaxValue = 100;
        public const int MinValue = 0;

        #region AbstractParameter Implementation

        public override string Name { get { return name; } }
        public override string ShortTypeName { get { return TypeName; } }
        public override string Unit { get { return unit; } }
        public override bool IsValid
        {
            get { return Value <= MaxValue && Value >= MinValue; }
        }
        public override bool Zeroable { get { return zeroable; } }

        #endregion // AbstractParameter Implementation

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            Humidity other = obj as Humidity;

            if (other != null)
                return this.Value.CompareTo(other.Value);
            else
                throw new ArgumentException("Object is not a " + this.GetType().Name + " .");
        }
        #endregion // IComparable Implementation

        public static Cloudiness FromDouble(string value)
        {
            double doubleValue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            return new Cloudiness(doubleValue);
        }

        public static Cloudiness FromRange(string value, int maxPossibleValue)
        {
            int normalized = PercentParameterConverter.Normalize(int.Parse(value), maxPossibleValue);
            return new Cloudiness(normalized);
        }

        public static Cloudiness FromPart(double value)
        {
            int normalized = PercentParameterConverter.Normalize(value);
            return new Cloudiness(normalized);
        }

        internal static Cloudiness FromMetarReportInFt(IList<CloudMetarDescription> skyDesription)
        {
            int aggregated = CloudinessConverter.AggregateMetarDescrition(skyDesription);
            return new Cloudiness(aggregated);
        }

        public Cloudiness(double value)
            : base(value)
        {
        }
        public Cloudiness(int value)
            : base(value)
        {
        }
        public Cloudiness(string value)
            : base(value)
        {
        }
    }
}
