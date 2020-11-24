using Blaggard;
using Blaggard.Common;

namespace WasteDrudgers.Level
{
    public static class MapUtils
    {
        public static (int w, int h) RoomDimensions(int min, int max) => (RNG.IntInclusive(min, max), RNG.IntInclusive(min, max));

        public static void Fill(Map map, Tile tile)
        {
            Rect(map, new Rect(0, 0, map.width, map.height), tile);
        }

        public static void Rect(Map map, Rect rect, Tile tile)
        {
            for (int x = rect.x; x < rect.x2; x++)
            {
                for (int y = rect.y; y < rect.y2; y++)
                {
                    SetCell(map, x, y, tile);
                }
            }
        }

        public static void SetCell(Map map, int x, int y, Tile tile)
        {
            map[Util.IndexFromXY(x, y, map.width)].Tile = tile;
        }
    }
}