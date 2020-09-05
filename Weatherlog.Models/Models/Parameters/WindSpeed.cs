using System;

namespace Weatherlog.Models.Parameters
{
    public class WindSpeed : AbstractParameter, IComparable
    {
        private static string name = "Скорость ветра";
        private static bool zeroable = true;
        private static string unit = "м/с";

        public const string TypeName = "windSpeed";
        public const int MaxValue = 150;
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
            WindSpeed other = obj as WindSpeed;

            if (other != null)
                return this.Value.CompareTo(other.Value);
            else
                throw new ArgumentException("Object is not a " + this.GetType().Name + " .");
        }
        #endregion // IComparable Implementation

        public static WindSpeed FromDouble(string value)
        {
            double doubleValue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            return new WindSpeed(doubleValue);
        }

        public static WindSpeed FromKmph(string value)
        {
            return new WindSpeed(WindSpeedConverter.KmphToMps(double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
        }

        public static WindSpeed FromKnot(string value)
        {
            return new WindSpeed(WindSpeedConverter.KnotToMps(double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
        }

        public WindSpeed(double value)
            : base(value)
        {
        }
        public WindSpeed(int value)
            : base(value)
        {
        }
        public WindSpeed(string value)
            : base(value)
        {
        }

    }
}
