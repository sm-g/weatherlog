using System;


namespace Weatherlog.Models.Parameters
{
    public class WindDirection : AbstractParameter
    {
        private static string name = "Направление ветра";
        private static bool zeroable = true;
        private static string unit = "°";
        static int middleValue = WindDirectionConverter.MAX_VALUE / 2;

        public const string TypeName = "windDir";
        public const int MaxValue = WindDirectionConverter.MAX_VALUE;
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

        public string Code { get { return WindDirectionConverter.ValueToCode(Value); } }

        public static int Subtract(int a, int b)
        {
            int d = Math.Abs(a - b);
            if (d > middleValue)
            {
                d = middleValue - (d % middleValue);
            }
            return d;
        }

        public static WindDirection FromDouble(string value)
        {
            double doubleValue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            return new WindDirection(doubleValue);
        }

        public static WindDirection FromRange(string value, int maxPossibleValue)
        {
            int normalized = WindDirectionConverter.Normalize(int.Parse(value), maxPossibleValue);
            return new WindDirection(normalized);
        }

        public static WindDirection FromCode(string code)
        {
            return new WindDirection(WindDirectionConverter.CodeToValue(code));
        }

        public WindDirection(double value)
        {
            Value = (int)Math.Round(value);
        }
        public WindDirection(int value)
            : base(value)
        {
        }
        public WindDirection(string value)
            : base(value)
        {
        }
    }
}
