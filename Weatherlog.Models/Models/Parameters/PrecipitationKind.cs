
namespace Weatherlog.Models.Parameters
{
    public class PrecipitationKind : AbstractParameter
    {
        private static string name = "Осадки, тип";
        private static bool zeroable = true;
        private static string unit = "";

        public const string TypeName = "precipKind";
        public const int MaxValue = PrecipitationKindConverter.max;
        public const int MinValue = PrecipitationKindConverter.min;

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

        public static PrecipitationKind FromGismeteoBased(string code)
        {
            return new PrecipitationKind(PrecipitationKindConverter.GismeteoCodeToValue(code));
        }
        public static PrecipitationKind FromMetar(string wx)
        {
            return new PrecipitationKind(PrecipitationKindConverter.MetarToValue(wx));
        }

        public PrecipitationKind(int value)
            : base(value)
        {
        }
    }
}
