using System;
using System.Collections.Generic;
using System.Linq;
using Blaggard;
using Blaggard.Common;
using WasteDrudgers.Common;

namespace WasteDrudgers.Level.Generation
{
    public class Intersection : Room
    {
        private static readonly string[] i7_1 = new[]
        {
            "### ###",
            "##   ##",
            "#     #",
            "       ",
            "#     #",
            "##   ##",
            "### ###",
        };
        private static readonly string[] i7_2 = new[]
        {
            "       ",
            " #   # ",
            "       ",
            " #   # ",
            "       ",
            " #   # ",
            "       ",
        };
        private static readonly string[] i5_1 = new[]
        {
            "     ",
            " # # ",
            "     ",
            " # # ",
            "     ",
        };
        private static readonly string[] i5_2 = new[]
        {
            "     ",
            "  #  ",
            " ### ",
            "  #  ",
            "     ",
        };
        private static readonly string[] i3_1 = new[]
        {
            "   ",
            " # ",
            "   ",
        };
        private static readonly string[][] templates_7 = new[] { i7_1, i7_2 };
        private static readonly string[][] templates_5 = new[] { i5_1, i5_2 };
        private static readonly string[][] templates_3 = new[] { i3_1 };

        public Intersection(Rect area) : base(area) { }
        public Intersection(Rect area, Direction from) : base(area, from) { }

        public (Room, Intersection) CreateCorridor(Direction direction, int width, int length, int intersectSize)
        {
            var iSize = intersectSize;
            var opposite = direction.GetOpposite();
            if (direction == Direction.North)
            {
                var x = Area.x + (Area.width - width) / 2;
                var y = Area.y - length;
                return
                (
                    new Room(new Rect(x, y + iSize, width, length - iSize), opposite),
                    new Intersection(new Rect(x - (iSize - width) / 2, y, iSize, iSize), opposite)
                );
            }
            else if (direction == Direction.West)
            {
                var x = Area.x - length;
                var y = Area.y + (Area.height - width) / 2;
                return
                (
                    new Room(new Rect(x + iSize, y, length - iSize, width), opposite),
                    new Intersection(new Rect(x, y - (iSize - width) / 2, iSize, iSize), opposite)
                );
            }
            else if (direction == Direction.East)
            {
                var x = Area.x + Area.width;
                var y = Area.y + (Area.height - width) / 2;
                return
                (
                    new Room(new Rect(x, y, length - iSize, width), opposite),
                    new Intersection(new Rect(x + length - iSize, y - (iSize - width) / 2, iSize, iSize), opposite)
                );
            }
            else if (direction == Direction.South)
            {
                var x = Area.x + (Area.width - width) / 2;
                var y = Area.y + Area.height;
                return
                (
                    new Room(new Rect(x, y, width, length - iSize), opposite),
                    new Intersection(new Rect(x - (iSize - width) / 2, y + length - iSize, iSize, iSize), opposite)
                );
            }
            return (null, null);
        }

        public override void Construct(Map map, Tile wall, Tile floor, bool constructWalls = false)
        {
            if (RNG.Int(100) < 25 && Area.width >= 3)
            {
                if (Area.width == 7)
                {
                    var t = templates_7[RNG.Int(templates_7.Length)];
                    MapUtils.PutTemplate(map, Area.x, Area.y, t, wall, floor);
                }
                else if (Area.width == 5)
                {
                    var t = templates_5[RNG.Int(templates_5.Length)];
                    MapUtils.PutTemplate(map, Area.x, Area.y, t, wall, floor);
                }
                else if (Area.width == 3)
                {
                    var t = templates_3[RNG.Int(templates_3.Length)];
                    MapUtils.PutTemplate(map, Area.x, Area.y, t, wall, floor);
                }
            }
            else
            {
                MapUtils.Rect(map, Area, floor);
            }
        }
    }

    public enum RoomConnection
    {
        Open,
        Closed,
        Tried,
    }

    // Rooms could have two Rects, one for floorspace and other for bounds
    public class Room
    {
        private Vec2 doorPos = Vec2.Zero;
        private RoomConnection[] connections = new[]
        {
            RoomConnection.Open,
            RoomConnection.Open,
            RoomConnection.Open,
            RoomConnection.Open,
        };

        public Rect Area { get; private set; }
        public bool HasOpen => connections.Any(r => r == RoomConnection.Open);

        private List<Direction> GetOpen()
        {
            var list = new List<Direction>();
            for (int i = 0; i < 4; i++)
            {
                if (connections[i] == RoomConnection.Open)
                {
                    list.Add(IndexToDirection(i));
                }
            }
            return list;
        }

        private Direction IndexToDirection(int i) => i switch
        {
            0 => Direction.North,
            1 => Direction.West,
            2 => Direction.East,
            3 => Direction.South,
        };
        private int DirectionToIndex(Direction dir) => dir switch
        {
            Direction.North => 0,
            Direction.West => 1,
            Direction.East => 2,
            Direction.South => 3,
        };

        public bool TryGetOpenDirection(out Direction dir)
        {
            var open = GetOpen();
            if (open.Count() > 0)
            {
                dir = open[RNG.Int(open.Count)];
                connections[DirectionToIndex(dir)] = RoomConnection.Tried;
                return true;
            }
            dir = default(Direction);
            return false;
        }

        public void SetClosed(Direction dir)
        {
            connections[DirectionToIndex(dir)] = RoomConnection.Closed;
        }

        public Room(Rect area) => Area = area;

        public Room(Rect area, Direction from)
        {
            Area = area;
            connections[DirectionToIndex(from)] = RoomConnection.Closed;
        }

        public Room(Rect area, Direction from, Vec2 doorPos)
        {
            Area = area;
            connections[DirectionToIndex(from)] = RoomConnection.Closed;
            this.doorPos = doorPos;
        }

        public void OpenTriedConnections()
        {
            for (int i = 0; i < 4; i++)
            {
                if (connections[i] == RoomConnection.Tried)
                {
                    connections[i] = RoomConnection.Open;
                }
            }
        }

        public Room GrowRoom(Direction direction, int roomWidth, int roomHeight)
        {
            var opposite = direction.GetOpposite();
            if (direction == Direction.North)
            {
                var doorX = RNG.Int(Area.x, Area.x2);
                var d = new Vec2(doorX, Area.y - 1);

                var x = RNG.IntInclusive(doorX - roomWidth + 1, doorX);
                var y = Area.y - roomHeight - 1;

                return new Room(new Rect(x, y, roomWidth, roomHeight), opposite, d);
            }
            else if (direction == Direction.West)
            {
                var x = Area.x - roomWidth - 1;

                var doorY = RNG.Int(Area.y, Area.y2);
                var d = new Vec2(Area.x - 1, doorY);

                var y = RNG.IntInclusive(doorY - roomHeight + 1, doorY);
                return new Room(new Rect(x, y, roomWidth, roomHeight), opposite, d);
            }
            else if (direction == Direction.East)
            {
                var x = Area.x + Area.width + 1;

                var doorY = RNG.Int(Area.y, Area.y2);
                var d = new Vec2(x - 1, doorY);

                var y = RNG.IntInclusive(doorY - roomHeight + 1, doorY);
                return new Room(new Rect(x, y, roomWidth, roomHeight), opposite, d);
            }
            else if (direction == Direction.South)
            {
                var doorX = RNG.Int(Area.x, Area.x2);

                var x = RNG.IntInclusive(doorX - roomWidth + 1, doorX);
                var y = Area.y + Area.height + 1;

                var d = new Vec2(doorX, y - 1);
                return new Room(new Rect(x, y, roomWidth, roomHeight), opposite, d);
            }
            return null;
        }

        public virtual void Construct(Map map, Tile wall, Tile floor, bool constructWalls = false)
        {
            var bounds = Area.Expand(1);
            if (constructWalls)
            {
                MapUtils.Rect(map, bounds, wall);
            }
            else
            {
                for (int y = bounds.y; y < bounds.y2; y++)
                {
                    for (int x = bounds.x; x < bounds.x2; x++)
                    {
                        var pos = map[Util.IndexFromXY(x, y, map.width)];
                        if (pos.Flags(TileFlags.BlocksMovement))
                        {
                            MapUtils.SetCell(map, x, y, wall);
                        }
                    }
                }
            }

            MapUtils.Rect(map, Area, floor);
            if (doorPos != Vec2.Zero)
            {
                MapUtils.SetCell(map, doorPos.x, doorPos.y, floor);
            }
        }
    }

    public class DungeonBuilder
    {
        private Map map;

        private List<Room> allFeatures = new List<Room>();

        private List<Intersection> intersections = new List<Intersection>();
        private List<Room> corridors = new List<Room>();
        private List<Room> rooms = new List<Room>();

        private Tile defaultWall;
        private Tile defaultFloor;

        public DungeonBuilder(Map map, Tile defaultWall, Tile defaultFloor)
        {
            this.map = map;
            this.defaultWall = defaultWall;
            this.defaultFloor = defaultFloor;
            var pos = new Vec2(RNG.Int(map.Width - 7), RNG.Int(map.Height - 7));
            AddIntersection(new Intersection(new Rect(pos.x, pos.y, 7, 7)));
        }

        public void ConstructCorridors()
        {
            foreach (var feature in corridors.Concat(intersections))
            {
                feature.Construct(map, defaultWall, defaultFloor);
            }
        }

        public void ConstructRooms()
        {
            foreach (var feature in rooms)
            {
                var wall = RNG.Int(100) < 50 ? defaultWall : Data.GetTile("brick_wall");
                var floor = RNG.Int(100) < 50 ? defaultFloor : Data.GetTile("stone_floor");
                feature.Construct(map, wall, floor, true);
            }
        }

        public void DrunkardReplacer(int diggers, int iterations, Tile replace, Tile floor)
        {
            for (int i = 0; i < diggers; i++)
            {
                foreach (var step in MapUtils.DrunkardWalk(map, iterations))
                {
                    var pos = map[step];
                    if (pos.Tile == replace)
                    {
                        pos.Tile = floor;
                    }
                }
            }
        }

        public void DrunkardDigger(int diggers, int iterations, Tile floor)
        {
            for (int i = 0; i < diggers; i++)
            {
                foreach (var step in MapUtils.DrunkardWalk(map, iterations))
                {
                    map[step].Tile = floor;
                }
            }
        }

        public void GenerateCorridors(int passes, int iterations, int target, Extent length)
        {
            var maxSize = 5;
            for (int p = 0; p < passes; p++)
            {
                // Loop until either iterations are used up or target room count reached
                for (int i = 0; i < iterations && corridors.Count < target; i++)
                {
                    var current = intersections[RNG.Int(intersections.Count)];
                    if (!current.HasOpen) continue;

                    var cSize = Math.Min(RNG.Odd(2), maxSize);
                    var iSize = RNG.Int(100) < 50 ? cSize : cSize + 2;
                    iSize = Math.Max(3, iSize);
                    maxSize = Math.Min(5, iSize);

                    if (current.TryGetOpenDirection(out var direction))
                    {
                        var (corridor, intersection) = current.CreateCorridor
                        (
                            direction,
                            cSize,
                            RNG.Extent(length) + iSize,
                            iSize
                        );

                        if (!IsValid(corridor, current)) continue;

                        current.SetClosed(direction);
                        AddCorridor(corridor);

                        if (!IsValid(intersection, corridor, current)) continue;

                        AddIntersection(intersection);
                    }
                }
                // Open each tried intersection for reuse
                foreach (var r in intersections)
                {
                    r.OpenTriedConnections();
                }
            }
        }

        public void GenerateRooms(int passes, int iterations, int target, Extent size)
        {
            for (int p = 0; p < passes; p++)
            {
                // Loop until either iterations are used up or target room count reached
                for (int i = 0; i < iterations && rooms.Count < target; i++)
                {
                    var current = allFeatures[RNG.Int(allFeatures.Count)];
                    if (current.TryGetOpenDirection(out var direction))
                    {
                        var room = current.GrowRoom(direction, RNG.Extent(size), RNG.Extent(size));

                        if (!IsRoomValid(room)) continue;

                        current.SetClosed(direction);
                        AddRoom(room);
                    }
                }
                foreach (var r in allFeatures)
                {
                    r.OpenTriedConnections();
                }
            }
        }

        private void AddRoom(Room room)
        {
            allFeatures.Add(room);
            rooms.Add(room);
        }

        private void AddCorridor(Room corridor)
        {
            allFeatures.Add(corridor);
            corridors.Add(corridor);
        }

        private void AddIntersection(Intersection intersection)
        {
            allFeatures.Add(intersection);
            intersections.Add(intersection);
        }

        private bool IsRoomValid(Room room, params Room[] ignore) =>
            room != null &&
            !room.Area.OutOfBounds(1, 1, map.width, map.height) &&
            !Overlaps(room.Area.Expand(1), ignore);

        private bool IsValid(Room feature, params Room[] ignore) =>
            feature != null &&
            !feature.Area.OutOfBounds(1, 1, map.width, map.height) &&
            !Overlaps(feature.Area.Expand(1), ignore);

        private bool Overlaps(Rect rect, Room[] ignore) => allFeatures.Any(i =>
        {
            if (ignore != null && ignore.Contains(i)) return false;

            return rect.IsOverlapping(i.Area);
        });

        private bool OverlapsCorridors(Rect rect) =>
            corridors.Any(i => rect.IsOverlapping(i.Area));

        private bool OverlapsRooms(Rect rect) =>
            rooms.Any(i => rect.IsOverlapping(i.Area));

        private bool OverlapsIntersections(Rect rect) =>
            intersections.Any(i => rect.IsOverlapping(i.Area));
    }

    public class DungeonStrategy : ILevelGenerationStrategy
    {
        private const int PASSES = 5;
        private const int MAX_ITERATIONS = 1000;

        public Extent RoomSize { get; set; }
        public Extent CorridorLength { get; set; }
        public int MaxCorridors { get; set; }
        public int MaxRooms { get; set; }

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

            MapUtils.Rect(map, 0, 0, Width, Height, wall);

            var builder = new DungeonBuilder(map, wall, floor);
            builder.GenerateCorridors(PASSES, MAX_ITERATIONS, MaxCorridors, CorridorLength);
            builder.GenerateRooms(PASSES, MAX_ITERATIONS, MaxRooms, RoomSize);

            builder.ConstructCorridors();

            builder.DrunkardDigger(RNG.IntInclusive(2, 5), RNG.IntInclusive(500, 1000), floor);

            var mb = new MapBuilder(map);
            mb.CellularAutomata(1, wall, floor, TileFlags.BlocksMovement);
            builder.ConstructCorridors();

            // Generate water
            builder.DrunkardReplacer(RNG.IntInclusive(0, 2), RNG.IntInclusive(500, 1000), floor, Data.GetTile("murky_water"));

            // Generate grass
            builder.DrunkardReplacer(RNG.IntInclusive(0, 2), RNG.IntInclusive(500, 1000), floor, Data.GetTile("mossy_floor"));

            // Mossy walls
            builder.DrunkardReplacer(RNG.IntInclusive(0, 2), RNG.IntInclusive(500, 1000), wall, Data.GetTile("mossy_wall"));

            builder.ConstructRooms();

            MapUtils.PrintMap(map);
        }
    }
}

