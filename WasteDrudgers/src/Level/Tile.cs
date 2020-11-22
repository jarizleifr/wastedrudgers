using System;
using Blaggard.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace WasteDrudgers.Level
{
    [Flags]
    public enum TileFlags
    {
        None,
        BlocksVision,
        BlocksMovement
    }

    /// <summary>
    /// An immutable map tile. Tile internals can never be changed, you must always replace the entire tile with another one.
    /// </summary>
    public class Tile
    {
        [JsonProperty("Index")]
        public readonly int index;
        [JsonProperty("Characters")]
        public readonly string characters;
        [JsonProperty("Flags")]
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly TileFlags flags;
        [JsonProperty("Foreground")]
        public readonly Color foreground;
        [JsonProperty("Background")]
        public readonly Color background;
        [JsonProperty("Description")]
        public readonly string description;
    }
}