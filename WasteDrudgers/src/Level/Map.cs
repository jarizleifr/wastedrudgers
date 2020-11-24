using System.Collections.Generic;
using Blaggard;
using Blaggard.Common;
using Microsoft.Extensions.ObjectPool;
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
        List<IMapCell> Neighbors { get; }
        IMapCell RandomNeighbor { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Map
    {
        private static ObjectPool<Cell> pool =
            new DefaultObjectPool<Cell>(new DefaultPooledObjectPolicy<Cell>(), 256);

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

        public int Width => width;
        public int Height => height;

        /// <summary>A map accessor.</summary>
        public class Cell : IMapCell
        {
            private static IMapCell NULL = new NullCell();
            private class NullCell : IMapCell
            {
                public Tile Tile { get => null; set { } }
                public Visibility Visibility { get => Visibility.Hidden; set { } }
                public BloodType Blood { get => BloodType.None; set { } }
                public bool Flags(TileFlags flags) => false;
                public List<IMapCell> Neighbors => null;
                public IMapCell RandomNeighbor => null;
            }

            private static readonly Vec2[] matrix = new[] { new Vec2(-1, -1), new Vec2(0, -1), new Vec2(1, -1), new Vec2(-1, 0), new Vec2(1, 0), new Vec2(-1, 1), new Vec2(0, 1), new Vec2(1, 1) };
            private Map map;
            private int index;

            public static IMapCell FromVec2(Map map, Vec2 vec2)
            {
                if (Util.IsWithinBounds(vec2.x, vec2.y, map.width, map.height))
                {
                    var cell = Map.pool.Get();
                    cell.map = map;
                    cell.index = vec2.ToIndex(map.width);
                    return cell;
                }
                else
                {
                    return NULL;
                }
            }

            public static IMapCell FromIndex(Map map, int index)
            {
                if ((index >= 0 && index < map.tiles.Length))
                {
                    var cell = Map.pool.Get();
                    cell.map = map;
                    cell.index = index;
                    return cell;
                }
                else
                {
                    return NULL;
                }
            }

            ~Cell()
            {
                Map.pool.Return(this);
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

            public List<IMapCell> Neighbors
            {
                get
                {
                    if (map == null) return null;
                    var n = new List<IMapCell>();
                    for (int i = 0; i < matrix.Length; i++)
                    {
                        n.Add(map[index + matrix[i].ToIndex(map.width)]);
                    }
                    return n;
                }
            }

            public IMapCell RandomNeighbor =>
                map[Vec2.FromIndex(index, map.width) + matrix[RNG.Int(8)]];

            public bool Flags(TileFlags flags) => (flags & map.tiles[index].flags) != 0;
        }
    }
}