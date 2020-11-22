using System;
using System.Collections.Generic;
using Blaggard.Common;

namespace WasteDrudgers.Level.Generation
{
    public class MazeStrategy : ILevelGenerationStrategy
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public string Floor { get; set; }
        public string Wall { get; set; }

        private Tile floor;
        private Tile wall;

        public void Generate(World world, string levelName, ref Map map)
        {
            if (map == null)
            {
                map = new Map(levelName, Width, Height);
            }

            floor = Data.GetTile(Floor);
            wall = Data.GetTile(Wall);

            RNG.Seed(map.seed);

            MapUtils.Fill(map, wall);

            var origin = new Vec2(1, 1);
            var digger = new Digger(map, origin);
            MapUtils.SetCell(map, origin.x, origin.y, floor);

            while (digger.Next(map, floor)) ;

            for (int y = 2; y < map.height - 4; y++)
            {
                for (int x = 2; x < map.width - 4; x++)
                {
                    var pos = new Vec2(x, y);

                    if (IsDeadEnd(map, pos))
                    {
                        Console.WriteLine("found dead end, demolishing");
                        MapUtils.SetCell(map, pos.x, pos.y, floor);
                    }

                    if (IsKnob(map, pos))
                    {
                        Console.WriteLine("found knob, demolishing");
                        //MapUtils.SetCell(map, pos.x, pos.y, floor);
                    }
                }
            }

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

        public bool IsDeadEnd(Map map, Vec2 pos)
        {
            var matrix = new NeighborMatrix(pos, (Vec2 v) => map[v].Flags(TileFlags.BlocksMovement));
            return (matrix.IsSet(0xff - 2 - 32) || matrix.IsSet(0xff - 128 - 8));
        }

        public bool IsKnob(Map map, Vec2 pos)
        {
            if (!map[pos].Flags(TileFlags.BlocksMovement)) return false;

            var matrix = new NeighborMatrix(pos, (Vec2 v) => map[v].Flags(TileFlags.BlocksMovement));
            return (matrix.IsSet(0xff - 1 - 2 - 4 - 8 - 128)
            || matrix.IsSet(0xff - 2 - 4 - 8 - 16 - 32)
            || matrix.IsSet(0xff - 8 - 16 - 32 - 64 - 128)
            || matrix.IsSet(0xff - 1 - 2 - 32 - 64 - 128));
        }

        public class Digger
        {
            private static Direction[] dirs = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
            private Stack<Vec2> visited;
            private Rect area;

            private Direction previous;

            public Digger(Map map, Vec2 origin)
            {
                visited = new Stack<Vec2>();
                visited.Push(origin);

                area = new Rect(1, 1, map.width - 2, map.height - 2);
            }

            public bool Next(Map map, Tile floor)
            {
                if (visited.Count == 0) return false;

                var available = new List<Direction>();

                var pos = visited.Peek();
                foreach (var dir in dirs)
                {
                    var tryPos = pos + Vec2.FromDirection(dir);
                    if (ValidPosition(map, tryPos, dir))
                    {
                        available.Add(dir);
                    }
                }

                if (available.Count == 0)
                {
                    visited.Pop();
                    return visited.Count > 0;
                }

                var direction = available[RNG.Int(available.Count)];
                var nextPos = pos + Vec2.FromDirection(direction);

                // If previously used direction is valid, have small chance to continue on same path
                if (RNG.Int(100) < 0)
                {
                    if (available.Contains(previous))
                    {
                        direction = previous;
                        nextPos = pos + Vec2.FromDirection(direction);
                    }
                }

                MapUtils.SetCell(map, nextPos.x, nextPos.y, floor);
                visited.Push(nextPos);

                previous = direction;
                return true;
            }

            private bool ValidPosition(Map map, Vec2 origin, Direction dir)
            {
                if (!map[origin].Flags(TileFlags.BlocksMovement)) return false;
                if (!area.IsWithinBounds(origin.x, origin.y)) return false;

                var matrix = new NeighborMatrix(origin, (Vec2 v) => map[v].Flags(TileFlags.BlocksMovement));
                return dir switch
                {
                    Direction.North => matrix.IsSet(1 + 2 + 4 + 8 + 128),
                    Direction.East => matrix.IsSet(2 + 4 + 8 + 16 + 32),
                    Direction.South => matrix.IsSet(8 + 16 + 32 + 64 + 128),
                    Direction.West => matrix.IsSet(1 + 2 + 32 + 64 + 128),
                };
            }
        }
    }
}
