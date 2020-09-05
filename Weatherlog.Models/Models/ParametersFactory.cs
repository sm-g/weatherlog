using System;
using System.Collections.Generic;

namespace Weatherlog.Models.Parameters
{
    public static class ParametersFactory
    {
        static List<string> knownParametersNames = new List<string>()
        {
            Temperature.TypeName,
            WindDirection.TypeName,
            WindSpeed.TypeName,
            PrecipitationAmount.TypeName,
            Humidity.TypeName,
            Cloudiness.TypeName,
            Pressure.TypeName,
            PrecipitationKind.TypeName,
        };
        static List<Type> knownParametersTypes = new List<Type>()
        {
            typeof(Temperature),
            typeof(WindDirection),
            typeof(WindSpeed),
            typeof(PrecipitationAmount),
            typeof(Humidity),
            typeof(Cloudiness),
            typeof(Pressure),
            typeof(PrecipitationKind)
        };

        public static IEnumerable<string> KnownParameterTypesNames
        {
            get
            {
                return knownParametersNames;
            }
        }

        public static IEnumerable<Type> KnownParametersTypes
        {
            get
            {
                return knownParametersTypes;
            }
        }

        public static bool IsPercentage(string type)
        {
            return type == Humidity.TypeName || type == Cloudiness.TypeName;
        }
        public static bool IsSpecialCaseValue(string type)
        {
            return type == PrecipitationAmount.TypeName;
        }
        public static bool IsZeroable(string type)
        {
            return CreateParameter(type, 1).Zeroable;
        }

        /// <summary>
        /// Creates new parameter by type name and value.
        /// </summary>
        /// <param name="type">Short name of parameter type.</param>
        /// <param name="value">Integer value of parameter.</param>
        /// <returns></returns>
        public static AbstractParameter CreateParameter(string type, int value)
        {
            switch (type)
            {
                case Temperature.TypeName:
                    return new Temperature(value);
                case WindDirection.TypeName:
                    return new WindDirection(value);
                case WindSpeed.TypeName:
                    return new WindSpeed(value);
                case PrecipitationAmount.TypeName:
                    return new PrecipitationAmount(value);
                case Humidity.TypeName:
                    return new Humidity(value);
                case Cloudiness.TypeName:
                    return new Cloudiness(value);
                case Pressure.TypeName:
                    return new Pressure(value);
                case PrecipitationKind.TypeName:
                    return new PrecipitationKind(value);
            }

            throw new ArgumentException("Unknown parameter type.", "type");
        }

        public static int GetMaxValue(string type)
        {
            switch (type)
            {
                case Temperature.TypeName:
                    return Temperature.MaxValue;
                case WindDirection.TypeName:
                    return WindDirection.MaxValue;
                case WindSpeed.TypeName:
                    return WindSpeed.MaxValue;
                case PrecipitationAmount.TypeName:
                    return PrecipitationAmount.MaxValue;
                case Humidity.TypeName:
                    return Humidity.MaxValue;
                case Cloudiness.TypeName:
                    return Cloudiness.MaxValue;
                case Pressure.TypeName:
                    return Pressure.MaxValue;
                case PrecipitationKind.TypeName:
                    return PrecipitationKind.MaxValue;
            }

            throw new ArgumentException("Unknown parameter type.", "type");
        }

        public static int GetMinValue(string type)
        {
            switch (type)
            {
                case Temperature.TypeName:
                    return Temperature.MinValue;
                case WindDirection.TypeName:
                    return WindDirection.MinValue;
                case WindSpeed.TypeName:
                    return WindSpeed.MinValue;
                case PrecipitationAmount.TypeName:
                    return PrecipitationAmount.MinValue;
                case Humidity.TypeName:
                    return Humidity.MinValue;
                case Cloudiness.TypeName:
                    return Cloudiness.MinValue;
                case Pressure.TypeName:
                    return Pressure.MinValue;
                case PrecipitationKind.TypeName:
                    return PrecipitationKind.MinValue;
            }

            throw new ArgumentException("Unknown parameter type.", "type");
        }

        public static int GetValueDistance(string type, int a, int b)
        {
            switch (type)
            {
                case WindDirection.TypeName:
                    return WindDirection.Subtract(a, b);
                default:
                    return Math.Abs(a - b);
            }
        }

        public static double GetRealDouble(string type, int value)
        {
            if (type == PrecipitationAmount.TypeName)
            {
                return PrecipitationAmountConverter.ToDouble(value);
            }
            return value;
        }

        public static string GetName(string type)
        {
            return CreateParameter(type, 0).Name;
        }

        public static string GetUnit(string type)
        {
            return CreateParameter(type, 0).Unit;
        }
    }
}
