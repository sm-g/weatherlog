using System;

namespace Weatherlog.Models.Parameters
{
    public class Humidity : AbstractParameter, IComparable
    {
        private static string name = "Влажность";
        private static bool zeroable = true;
        private static string unit = "%";

        public const string TypeName = "hum";
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

        public static Humidity FromDouble(string value)
        {
            double doubleValue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            return new Humidity((int)Math.Round(doubleValue));
        }

        public static Humidity FromPart(double value)
        {
            int normalized = PercentParameterConverter.Normalize(value);
            return new Humidity(normalized);
        }

        public static Humidity FromDewPoint(Temperature temperature, Temperature duePoint)
        {
            return new Humidity(HumidityConverter.FromDuePoint(temperature.Value, duePoint.Value));
        }

        public Humidity(int value)
            : base(value)
        {
        }

        public Humidity(string value)
            : base(value)
        {
        }
    }
}
