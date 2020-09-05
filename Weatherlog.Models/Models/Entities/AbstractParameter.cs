using Newtonsoft.Json;
using System;

namespace Weatherlog.Models.Parameters
{
    [JsonConverter(typeof(JsonParametersConverter))]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class AbstractParameter
    {
        [JsonProperty(PropertyName = JsonParametersConverter.Value_PropertyName)]
        public int Value { get; protected set; }

        [JsonProperty(PropertyName = JsonParametersConverter.Name_PropertyName)]
        public abstract string Name { get; }

        public abstract string ShortTypeName { get; }
        public abstract string Unit { get; }
        public abstract bool Zeroable { get; }
        public abstract bool IsValid { get; }

        public virtual string RealValueString
        {
            get
            {
                return Value.ToString();
            }
        }
        public override string ToString()
        {
            return String.Format("{0} {1}", RealValueString, Unit);
        }

        /// <summary>
        /// Initializes a new instance with Value given as it actually stored in object.
        /// Only when reading values from database.
        /// </summary>
        /// <param name="value"></param>
        public AbstractParameter(int value)
        {
            Value = value;

        }
        /// <summary>
        /// Initializes a new instance with Value given as string of actually stored in object value.
        /// </summary>
        /// <param name="value"></param>
        public AbstractParameter(string value)
            : this(int.Parse(value))
        {
        }
        /// <summary>
        /// Initializes a new instance with Value rounded to actually stored in object value.
        /// </summary>
        /// <param name="value"></param>
        public AbstractParameter(double value)
            : this((int)Math.Round(value))
        {
        }
        protected AbstractParameter() { }
    }
}
