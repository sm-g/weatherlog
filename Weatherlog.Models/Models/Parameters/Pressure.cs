using System;

namespace Weatherlog.Models.Parameters
{
    public class Pressure : AbstractParameter, IComparable
    {
        private static string name = "Давление";
        private static bool zeroable = false;
        private static string unit = "мм рт. ст.";

        public const string TypeName = "pressure";
        public const int MaxValue = 900;
        public const int MinValue = 500;

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
            Pressure other = obj as Pressure;

            if (other != null)
                return this.Value.CompareTo(other.Value);
            else
                throw new ArgumentException("Object is not a " + this.GetType().Name + " .");
        }
        #endregion // IComparable Implementation

        public static Pressure FromHpa(string value)
        {
            return new Pressure(PressureConverter.HpaToMmhg(double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
        }

        public static Pressure FromInhg(string value)
        {
            return new Pressure(PressureConverter.HpaToInhg(double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)));
        }

        public static Pressure FromHpa(double value)
        {
            return new Pressure(PressureConverter.HpaToMmhg(value));
        }

        public Pressure(int value)
            : base(value)
        {
        }
        public Pressure(string value)
            : base(value)
        {
        }
    }
}
