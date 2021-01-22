using Newtonsoft.Json;
namespace WasteDrudgers.Common
{
    public struct Attrib
    {
        public int Base { get; set; }
        public int Mod { get; set; }

        [JsonIgnore]
        public int Current => Base + Mod;

        public static Attrib operator ++(Attrib s) => s.Base += 1;
        public static Attrib operator --(Attrib s) => s.Base -= 1;

        public static implicit operator int(Attrib s) => s.Base + s.Mod;
        public static implicit operator Attrib(int s) => new Attrib { Base = s };
    }
}