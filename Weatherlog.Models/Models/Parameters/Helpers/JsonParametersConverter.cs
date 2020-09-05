using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Weatherlog.Models.Parameters
{
    class JsonParametersConverter : JsonConverter
    {
        public const string Name_PropertyName = "n";
        public const string Value_PropertyName = "v";

        public override bool CanConvert(Type objectType)
        {
            throw new JsonSerializationException("JsonParametersConverter custom converter should be used in attribute.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            int value = jObject.SelectToken(Value_PropertyName).Value<int>();
            string condtionName = jObject.SelectToken(Name_PropertyName).Value<string>();
            return ParametersFactory.CreateParameter(condtionName, value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            AbstractParameter ac = (AbstractParameter)value;
            writer.WriteStartObject();

            writer.WritePropertyName(Name_PropertyName);
            writer.WriteValue(ac.ShortTypeName);

            writer.WritePropertyName(Value_PropertyName);
            writer.WriteValue(ac.Value);

            writer.WriteEndObject();
        }
    }
}
