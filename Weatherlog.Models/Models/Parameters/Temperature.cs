using System;

namespace Weatherlog.Models.Parameters
{
    public class Temperature : AbstractParameter, IComparable
    {
        private static string name = "Температура";
        private static bool zeroable = true;
        private static string unit = "°C";

        public const string TypeName = "temp";
        public const int MaxValue = 70;
        public const int MinValue = -90;

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
            Temperature other = obj as Temperature;

            if (other != null)
                return this.Value.CompareTo(other.Value);
            else
                throw new ArgumentException("Object is not a " + this.GetType().Name + " .");
        }
        #endregion // IComparable Implementation

        public static Temperature FromDouble(string value)
        {
            double doubleValue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            return new Temperature(doubleValue);
        }

        public Temperature(double value)
            : base(value)
        {
        }
        public Temperature(int value)
            : base(value)
        {
        }
        public Temperature(string value)
            : base(value)
        {
        }
    }
}
