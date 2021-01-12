using Blaggard.Common;
using ManulECS;
using Newtonsoft.Json;

namespace WasteDrudgers
{
    public struct StatusInfo
    {
        public string text;
        public Color color;
    }

    [SerializationProfile("global")]
    public class PlayerData
    {
        public Entity entity;
        public string name;
        public string currentLevel;
        public int turns;

        // Temporary player specific stuff
        [JsonIgnore]
        public Vec2 coords;
        [JsonIgnore]
        public Entity? lastTarget;
    }
}