using System;

namespace Weatherlog.Models.Parameters
{
    public class PrecipitationAmount : AbstractParameter, IComparable
    {
        private static string name = "Осадки";
        private static bool zeroable = true;
        private static string unit = "мм";

        public const string TypeName = "precip";
        public const int MaxValue = 1000;
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
        public override string RealValueString
        {
            get { return PrecipitationAmountConverter.ToString(Value); }
        }

        #endregion // AbstractParameter Implementation

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            PrecipitationAmount other = obj as PrecipitationAmount;

            if (other != null)
                return this.Value.CompareTo(other.Value);
            else
                throw new ArgumentException("Object is not a Precipitation.");
        }
        #endregion // IComparable Implementation


        public PrecipitationAmount(double value)
        {
            Value = PrecipitationAmountConverter.ToValue(value);
        }
        public PrecipitationAmount(string str)
        {
            Value = PrecipitationAmountConverter.ToValue(str);
        }
    }
}
