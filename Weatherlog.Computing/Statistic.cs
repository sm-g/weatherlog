using System;
using System.Collections.Generic;
using System.Linq;
using Weatherlog.Models.Parameters;

namespace Weatherlog.Computing
{
    public enum StatisticMethods
    {
        Mae,
        Mase,
        Mape,
        Rmse,
        All
    }

    static class Statistic
    {
        public static event EventHandler<StatisticCalculatingFailedEventArgs> CalculatingFailed = delegate { };

        public static Dictionary<StatisticMethods, double> CalculateMany(IEnumerable<StatisticMethods> methods, ParameterTimeSeries forecast, ParameterTimeSeries real)
        {
            var result = new Dictionary<StatisticMethods, double>();
            if (methods.Contains(StatisticMethods.All))
            {
                methods = new List<StatisticMethods> {
                    StatisticMethods.Mae,
                    StatisticMethods.Mase,
                    StatisticMethods.Mape,
                    StatisticMethods.Rmse
                };
            }
            foreach (var method in methods.Distinct())
            {
                try
                {
                    result.Add(method, GetFunc(method)(forecast, real));
                }
                catch (Exception e)
                {
                    OnCalculatingFailed(method, e.Message);
                }
            }
            return result;
        }

        static Func<ParameterTimeSeries, ParameterTimeSeries, double> GetFunc(StatisticMethods method)
        {
            switch (method)
            {
                case StatisticMethods.Mae:
                    return (x, y) => MAE(x, y);
                case StatisticMethods.Mape:
                    return (x, y) => MAPE(x, y);
                case StatisticMethods.Mase:
                    return (x, y) => MASE(x, y);
                case StatisticMethods.Rmse:
                    return (x, y) => RMSE(x, y);
            }
            throw new ArgumentOutOfRangeException("Unknkown statictic method " + method);
        }

        public static double MAE(ParameterTimeSeries a, ParameterTimeSeries b)
        {
            EvaluateCorrectness(a, b, 1);

            string type = a.Type;
            int length = Math.Min(a.Length, b.Length);
            int sumOfErrors = 0;

            for (var i = 0; i < length; i++)
            {
                sumOfErrors += ParametersFactory.GetValueDistance(type, a.Values[i], b.Values[i]);
            }

            double result = (double)sumOfErrors / length;

            return result;
        }

        public static double MASE(ParameterTimeSeries forecast, ParameterTimeSeries real)
        {
            EvaluateCorrectness(forecast, real, 2);

            string type = forecast.Type;
            int length = Math.Min(forecast.Length, real.Length);

            int sumOfNaiveErrors = 0;
            int sumOfErrors = ParametersFactory.GetValueDistance(type, real.Values[0], forecast.Values[0]);

            for (var i = 1; i < length; i++)
            {
                sumOfErrors += ParametersFactory.GetValueDistance(type, real.Values[i], forecast.Values[i]);
                sumOfNaiveErrors += ParametersFactory.GetValueDistance(type, real.Values[i], real.Values[i - 1]);
            }

            double result = (double)sumOfErrors * (length - 1) / (length * sumOfNaiveErrors);

            return result;
        }

        public static double MAPE(ParameterTimeSeries forecast, ParameterTimeSeries real)
        {
            EvaluateCorrectness(forecast, real, 1);

            string type = forecast.Type;
            if (ParametersFactory.IsZeroable(type))
            {
                throw new ArgumentException("MAPE of zeroable values not allowed.");
            }

            int length = Math.Min(forecast.Length, real.Length);
            double sumOfPercantageErrors = 0;

            for (var i = 0; i < length; i++)
            {
                sumOfPercantageErrors += Math.Abs((double)ParametersFactory.GetValueDistance(type, real.Values[i], forecast.Values[i]) / real.Values[i]);
            }

            double result = sumOfPercantageErrors / length;

            return result;
        }

        public static double RMSE(ParameterTimeSeries forecast, ParameterTimeSeries real)
        {
            EvaluateCorrectness(forecast, real, 1);

            string type = forecast.Type;
            int length = Math.Min(forecast.Length, real.Length);
            double sumOfSquareErrors = 0;

            for (var i = 0; i < length; i++)
            {
                sumOfSquareErrors += Math.Pow((double)(ParametersFactory.GetValueDistance(type, real.Values[i], forecast.Values[i])), 2);
            }

            double result = Math.Sqrt(sumOfSquareErrors / length);

            return result;
        }

        private static void EvaluateCorrectness(ParameterTimeSeries a, ParameterTimeSeries b, int minimumElementsRequired)
        {
            if (a == null)
                throw new ArgumentNullException("a", "First argument is null");
            if (b == null)
                throw new ArgumentNullException("b", "Second argument is null");

            if (a.Type != b.Type)
                throw new ArgumentException("Series type mismatch.");

            if (a.Length < minimumElementsRequired)
                throw new ArgumentException("First series contains less than " + minimumElementsRequired + " elements.", "a");
            if (b.Length < minimumElementsRequired)
                throw new ArgumentException("Second series contains less than " + minimumElementsRequired + " elements.", "b");
        }

        private static void OnCalculatingFailed(StatisticMethods method, string message)
        {
            CalculatingFailed(null, new StatisticCalculatingFailedEventArgs(method, message));
        }
    }
}
