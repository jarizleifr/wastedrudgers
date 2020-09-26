using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WasteDrudgers.Entities
{
    // Stats deserialize by default very verbose, this helps to cut down on file size
    public class StatConverter : JsonConverter<Stat>
    {
        public override void WriteJson(JsonWriter writer, Stat value, JsonSerializer serializer)
        {
            new JArray(value.Base, value.Damage, value.Mod).WriteTo(writer);
        }
        public override Stat ReadJson(JsonReader reader, Type objectType, Stat existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var arr = (JArray)JToken.Load(reader);
            var stat = new Stat((int)arr[0]);
            stat.Damage = (int)arr[1];
            stat.Mod = (int)arr[2];
            return stat;
        }
    }

    [JsonConverter(typeof(StatConverter))]
    public struct Stat
    {
        private int val;
        private int dmg;
        private int mod;

        public Stat(int value)
        {
            this.val = value;
            this.dmg = 0;
            this.mod = 0;
        }

        public int Base
        {
            get => val;
            set
            {
                val = value;
                dmg = Math.Min(Max, dmg);
            }
        }
        public int Mod
        {
            get => mod;
            set => mod = value;
        }
        public int Damage
        {
            get => dmg;
            set => dmg = Math.Max(0, Math.Min(Max, value));
        }
        public int Current => val + mod - dmg;
        public int Max => val + mod;

        public static implicit operator int(Stat s) => s.Current;
        public static implicit operator Stat(int s) => new Stat(s);
    }
}