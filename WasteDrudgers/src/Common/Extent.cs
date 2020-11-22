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
            var str = JToken.Load(reader).ToString();
            var split = str.Split("-");
            return new Extent(int.Parse(split[0]), int.Parse(split[1]));
        }
    }

    [JsonConverter(typeof(ExtentConverter))]
    public readonly struct Extent
    {
        public readonly int min;
        public readonly int max;

        public Extent(int min, int max) =>
            (this.min, this.max) = (min, max);

        public override string ToString() =>
            $"{min}─{max}";
    }
}