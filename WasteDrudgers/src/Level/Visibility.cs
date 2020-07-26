using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;

namespace WasteDrudgers.Level
{
    public enum Visibility
    {
        Hidden,
        Explored,
        Visible
    }

    public class VisibilityArrayConverter : JsonConverter<Visibility[]>
    {
        public override void WriteJson(JsonWriter writer, Visibility[] value, JsonSerializer serializer)
        {
            var bools = value.Select(v => v != Visibility.Hidden).ToArray();
            var explored = new byte[(bools.Length + 7) / 8];
            int bitIndex = 0, byteIndex = 0;
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                {
                    explored[byteIndex] |= (byte)(((byte)1) << bitIndex);
                }
                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
            serializer.Serialize(writer, explored);
        }

        public override Visibility[] ReadJson(JsonReader reader, Type objectType, Visibility[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var bitArray = new BitArray(serializer.Deserialize<byte[]>(reader));
            var visibility = new Visibility[bitArray.Length];
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray[i])
                {
                    visibility[i] = Visibility.Explored;
                }
            }
            return visibility;
        }
    }
}