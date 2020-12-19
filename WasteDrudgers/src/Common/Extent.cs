using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WasteDrudgers.Common
{
    public class ExtentConverter : JsonConverter<Extent>
    {
        public override void WriteJson(JsonWriter writer, Extent value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.min}-{value.max}");
            writer.Flush();
        }

        public override Extent ReadJson(JsonReader reader, Type objectType, Extent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.String:
                    var split = token.ToString().Split("-");
                    return new Extent(int.Parse(split[0]), int.Parse(split[1]));
                case JTokenType.Integer:
                    return new Extent(token.ToObject<int>());
            }
            throw new Exception("Cannot convert to Extent");
        }
    }

    [JsonConverter(typeof(ExtentConverter))]
    public readonly struct Extent
    {
        public readonly int min;
        public readonly int max;

        public Extent(int min, int max) =>
            (this.min, this.max) = (min, max);

        public Extent(int value) =>
            (this.min, this.max) = (value, value);

        public override string ToString() =>
            $"{min}─{max}";
    }
}