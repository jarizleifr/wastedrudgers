using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blaggard.Common;
using Newtonsoft.Json;

namespace WasteDrudgers.Level.Generation
{
    public class DungeonStrategy : ILevelGenerationStrategy
    {
        public int RoomChance { get; set; }
        public int MinRoomSize { get; set; }
        public int MaxRoomSize { get; set; }
        public int MinCorridorLength { get; set; }
        public int MaxCorridorLength { get; set; }
        public int MaxFeatures { get; set; }
        public int MaxIterations { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public string Floor { get; set; }
        public string Wall { get; set; }

        private List<Room> rooms;
        private List<Rect> corridors;

        private Tile floor;
        private Tile wall;

        public class Room
        {
            public readonly Rect rect;
            private List<Direction> open;

            public Room(Rect rect)
            {
                this.rect = rect;
                open = new List<Direction>() { Direction.South, Direction.West, Direction.East, Direction.North };
            }

            public bool HasOpen => open.Count > 0;
            public Direction GetOpenDirection() => open[RNG.Int(open.Count)];

            public Vec2 PickRandomWall(Direction dir) => dir switch
            {
                Direction.South => new Vec2(RNG.Int(rect.x + 1, rect.x2 - 2), rect.y2),
                Direction.West => new Vec2(rect.x - 1, RNG.Int(rect.y + 1, rect.y2 - 2)),
                Direction.East => new Vec2(rect.x2, RNG.Int(rect.y + 1, rect.y2 - 2)),
                Direction.North => new Vec2(RNG.Int(rect.x + 1, rect.x2 - 2), rect.y - 1),
                _ => throw new Exception("Invalid wall direction")
            };
        }

        public void Generate(World world, string levelName, ref Map map)
        {
            if (map == null)
            {
                map = new Map(levelName, Width, Height);
            }

            rooms = new List<Room>();

            floor = Data.GetTile(Floor);
            wall = Data.GetTile(Wall);

            RNG.Seed(map.seed);

            int initialRoomWidth = RNG.IntInclusive(MinRoomSize, MaxRoomSize);
            int initialRoomHeight = RNG.IntInclusive(MinRoomSize, MaxRoomSize);
            var initialRoomRect = new Rect(map.width / 2 - initialRoomWidth / 2, map.height / 2 - initialRoomHeight / 2, initialRoomWidth, initialRoomHeight);

            MapUtils.Fill(map, wall);
            AddFeature(map, initialRoomRect);

            int i = 0;
            while (rooms.Count < MaxFeatures && i < MaxIterations)
            {
                var room = rooms[RNG.Int(rooms.Count)];
                if (!room.HasOpen) continue;

                GrowNewFeatureFrom(map, room);
                i++;
            }

            i = 0;
            while (i < 1000)
            {
                TryAddDoor(map);
                i++;
            }
        }

        public void GrowNewFeatureFrom(Map map, Room room)
        {
            var dir = room.GetOpenDirection();
            var pos = room.PickRandomWall(dir);
            var newRoomSize = MapUtils.RoomDimensions(MinRoomSize, MaxRoomSize);

            Rect newRoomRect = dir switch
            {
                Direction.South => new Rect(RNG.IntInclusive(pos.x - newRoomSize.w + 2, pos.x), pos.y + 1, newRoomSize.w, newRoomSize.h),
                Direction.West => new Rect(pos.x - newRoomSize.w, RNG.IntInclusive(pos.y - newRoomSize.h + 2, pos.y), newRoomSize.w, newRoomSize.h),
                Direction.East => new Rect(pos.x + 1, RNG.IntInclusive(pos.y - newRoomSize.h + 2, pos.y), newRoomSize.w, newRoomSize.h),
                Direction.North => new Rect(RNG.IntInclusive(pos.x - newRoomSize.w + 2, pos.x), pos.y - newRoomSize.h, newRoomSize.w, newRoomSize.h)
            };

            if (rooms.All(r => !r.rect.Expand(1).IsOverlapping(newRoomRect)))
            {
                MapUtils.Rect(map, newRoomRect, floor);
                MapUtils.SetCell(map, pos.x, pos.y, floor);
                rooms.Add(new Room(newRoomRect));
            }
        }

        private Room AddFeature(Map map, Rect rect)
        {
            var room = new Room(rect);
            MapUtils.Rect(map, rect, floor);
            rooms.Add(room);
            return room;
        }

        private void TryAddDoor(Map map)
        {
            var pos = GetRandomPosition(map);

            if (!map[pos].Flags(TileFlags.BlocksMovement)) return;

            var left = pos + new Vec2(-1, 0);
            var right = pos + new Vec2(1, 0);

            var up = pos + new Vec2(0, -1);
            var down = pos + new Vec2(0, 1);

            if ((!map[left].Flags(TileFlags.BlocksMovement) && !map[right].Flags(TileFlags.BlocksMovement)) || (!map[left].Flags(TileFlags.BlocksMovement) && !map[right].Flags(TileFlags.BlocksMovement)))
            {
                Console.WriteLine("Found suitable spot! Adding doorway");
                MapUtils.SetCell(map, pos.x, pos.y, floor);
            }
        }

        private Vec2 GetRandomPosition(Map map) => new Vec2(RNG.Int(0, map.width), RNG.Int(0, map.height));
    }
}
