using System;
using Blaggard.Common;
using Newtonsoft.Json;
namespace WasteDrudgers.Level
{
    [Flags]
    public enum TileFlags
    {
        None,
        BlocksVision,
        BlocksMovement
    }

    // TODO: new database model broke immutability, find a way to prevent changing fields (and all other DB classes)

    /// <summary>
    /// An immutable map tile. Tile internals can never be changed, you must always replace the entire tile with another one.
    /// </summary>
    public class Tile
    {
        public readonly string characters;
        public char glyph;
        public readonly TileFlags flags;

        [JsonIgnore]
        public Color foreground;
        [JsonIgnore]
        public Color background;

        public readonly string description;
        public readonly string sprite;
        public readonly bool animated;

        public Tile(string characters, char glyph, TileFlags flags, string description, string sprite, bool animated)
        {
            this.characters = characters;
            this.glyph = glyph;
            this.flags = flags;
            this.description = description;
            this.sprite = sprite;
            this.animated = animated;
        }
    }
}