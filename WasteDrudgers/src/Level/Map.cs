using System.Collections.Generic;
using System.Linq;

using Blaggard;
using Blaggard.Common;
using Newtonsoft.Json;
using WasteDrudgers.Entities;

namespace WasteDrudgers.Level
{
    public interface IMapCell
    {
        Tile Tile { get; set; }
        Visibility Visibility { get; set; }
        BloodType Blood { get; set; }
        bool Flags(TileFlags flags);

        IEnumerable<IMapCell> Neighbors { get; }
        IMapCell RandomNeighbor { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Map
    {
        [JsonProperty]
        public readonly string name;
        [JsonProperty]
        public readonly int width, height;
        [JsonProperty]
        public readonly int seed;

        [JsonProperty]
        [JsonConverter(typeof(VisibilityArrayConverter))]
        private Visibility[] visibility;

        private BloodType[] blood;

        private Tile[] tiles;

        public Map(string name, int width, int height)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            seed = RNG.Int(int.MaxValue);
            tiles = new Tile[width * height];
            visibility = new Visibility[width * height];
            blood = new BloodType[width * height];
        }

        public IMapCell this[Vec2 vec2] => Cell.FromVec2(this, vec2);
        public IMapCell this[int index] => Cell.FromIndex(this, index);

        public Tile[] GetTiles() => tiles;
        public Visibility[] GetVisibility() => visibility;
        public BloodType[] GetBlood() => blood;

        /// <summary>A map accessor.</summary>
        private class Cell : IMapCell
        {
            private readonly static IMapCell NULL = new NullCell();
            private class NullCell : IMapCell
            {
                public Tile Tile { get => null; set { } }
                public Visibility Visibility { get => default(Visibility); set { } }
                public BloodType Blood { get => default(BloodType); set { } }
                public bool Flags(TileFlags flags) => true;
                public IEnumerable<IMapCell> Neighbors => Enumerable.Empty<IMapCell>();
                public IMapCell RandomNeighbor => NULL;
            }

            private static readonly Vec2[] matrix = new[] { new Vec2(-1, -1), new Vec2(0, -1), new Vec2(1, -1), new Vec2(-1, 0), new Vec2(1, 0), new Vec2(-1, 1), new Vec2(0, 1), new Vec2(1, 1) };
            private readonly Map map;
            private readonly int index;

            public static IMapCell FromVec2(Map map, Vec2 vec2) => Util.IsWithinBounds(vec2.x, vec2.y, map.width, map.height)
                ? new Cell(map, vec2.ToIndex(map.width))
                : NULL;

            public static IMapCell FromIndex(Map map, int index) => (index >= 0 && index < map.tiles.Length)
                ? new Cell(map, index)
                : NULL;

            private Cell(Map map, int index)
            {
                this.map = map;
                this.index = index;
            }

            public Tile Tile
            {
                get => map.tiles[index];
                set => map.tiles[index] = value;
            }

            public Visibility Visibility
            {
                get => map.visibility[index];
                set => map.visibility[index] = value;
            }

            public BloodType Blood
            {
                get => map.blood[index];
                set => map.blood[index] = value;
            }

            public IEnumerable<IMapCell> Neighbors =>
                matrix.Select(m => map[index + m.ToIndex(map.width)]);

            public IMapCell RandomNeighbor =>
                map[Vec2.FromIndex(index, map.width) + matrix[RNG.Int(8)]];

            public bool Flags(TileFlags flags) => (flags & map.tiles[index].flags) != 0;
        }
    }
}