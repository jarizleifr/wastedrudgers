using System;
using System.Collections.Generic;
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

        public static void Rect(Map map, Rect rect, Tile tile) =>
            Rect(map, rect.x1, rect.y1, rect.x2, rect.y2, tile);

        public static void Rect(Map map, int x1, int y1, int x2, int y2, Tile tile)
        {
            for (int x = x1; x < x2; x++)
            {
                for (int y = y1; y < y2; y++)
                {
                    SetCell(map, x, y, tile);
                }
            }
        }

        public static void SetCell(Map map, int x, int y, Tile tile)
        {
            map[Util.IndexFromXY(x, y, map.width)].Tile = tile;
        }

        public static void PrintMap(Map map)
        {
            for (int y = 0; y < map.height; y++)
            {
                string line = "";
                for (int x = 0; x < map.width; x++)
                {
                    line += map[new Vec2(x, y)].Flags(TileFlags.BlocksMovement) ? '█' : ' ';
                }
                Console.WriteLine(line);
            }
        }

        public static void PutTemplate(Map map, int x, int y, string[] template, Tile wall, Tile floor)
        {
            for (int yy = 0; yy < template.Length; yy++)
            {
                for (int xx = 0; xx < template[0].Length; xx++)
                {
                    SetCell(map, x + xx, y + yy, template[yy][xx] == '#' ? wall : floor);
                }
            }
        }

        public static List<Vec2> DrunkardWalk(Map map, int steps)
        {
            var current = new Vec2(RNG.Int(map.width), RNG.Int(map.height));
            var list = new List<Vec2>() { current };
            while (steps > 0)
            {
                var x = current.x + RNG.IntInclusive(-1, 1);
                var y = current.y + RNG.IntInclusive(-1, 1);

                if (x == map.width - 1) { x -= 1; }
                if (y == map.height - 1) { y -= 1; }
                if (x == 0) { x += 1; }
                if (y == 0) { y += 1; }

                current = new Vec2(x, y);
                list.Add(current);

                steps--;
            }
            return list;
        }
    }
}