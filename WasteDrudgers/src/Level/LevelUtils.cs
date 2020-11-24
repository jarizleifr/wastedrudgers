using System;
using Blaggard.Common;

using WasteDrudgers.Entities;

namespace WasteDrudgers.Level
{
    //TODO: Rename to SpatialUtils? we have mostly just position queries here, probably moreso in the future
    public static class LevelUtils
    {
        public static Vec2 GetRandomPosition(Map map) => new Vec2(RNG.Int(0, map.width), RNG.Int(0, map.height));

        public static Vec2 GetRandomPassablePosition(World world)
        {
            var map = world.Map;
            var i = RNG.Int(0, map.width * map.height);
            while (map[i].Flags(TileFlags.BlocksMovement))
            {
                i = RNG.Int(0, map.width * map.height);
            }
            return Vec2.FromIndex(i, map.width);
        }

        public static Vec2 GetRandomPassablePositionWithoutCreature(World world)
        {
            var pos = GetRandomPassablePosition(world);
            while (world.spatial.TryGetCreature(pos, out var e))
            {
                pos = GetRandomPassablePosition(world);
            }
            return pos;
        }

        public static Vec2 GetRandomPassablePositionWithoutFeature(World world)
        {
            var pos = GetRandomPassablePosition(world);
            while (world.spatial.TryGetFeature(pos, out var e))
            {
                pos = GetRandomPassablePosition(world);
            }
            return pos;
        }

        public static Vec2 GetRandomPassableEmptyPosition(World world)
        {
            var pos = GetRandomPassablePosition(world);
            while (world.spatial.TryGetFeature(pos, out var f) && world.spatial.TryGetCreature(pos, out var c))
            {
                pos = GetRandomPassablePosition(world);
            }
            return pos;
        }

        private static Func<IMapCell, bool> callback =
            (c) => c.Flags(TileFlags.BlocksMovement);

        public static bool HasLineOfSight(World world, Vec2 origin, Vec2 target, int maxDistance = -1)
        {
            var map = world.Map;
            if (maxDistance != -1 && !origin.IsWithinCircle(target, maxDistance))
            {
                return false;
            }
            return Bresenham.CheckPath(map, origin, target, callback);
        }

        // TODO: Look needs visibility checks
        public static string GetLookDescription(World world, Vec2 coords)
        {
            var map = world.Map;
            var cell = map[coords];

            string description = null;
            if (world.spatial.TryGetCreature(coords, out var creature))
            {
                var identity = world.ecs.GetRef<Identity>(creature);
                description = world.ecs.Has<Player>(creature) ? "This ugly creature here is you." : $"There is {identity.name} here.";
            }

            var itemCount = world.spatial.ItemsCountAt(coords);
            if (itemCount == 1)
            {
                var item = world.spatial.GetItemOrThrow(coords);
                var name = Items.GetName(world, item);
                description = $"There's {name} here";
            }
            else if (itemCount > 1)
            {
                description = "There are several items here";
            }

            if (world.spatial.TryGetFeature(coords, out var feature))
            {
                var identity = world.ecs.GetRef<Identity>(feature);
                description = $"There's {identity.description} here.";
            }

            description ??= cell.Visibility switch
            {
                Visibility.Hidden => "There's nothing but impenetrable darkness here.",
                Visibility.Explored => $"You remember seeing {cell.Tile.description} here.",
                Visibility.Visible => cell.Tile.description,
                _ => null
            };

            return description;
        }

        public static string GetDescription(World world, Vec2 coords)
        {
            var map = world.Map;
            var cell = map[coords];

            string description = null;
            if (world.spatial.TryGetFeature(coords, out var feature))
            {
                var name = world.ecs.GetRef<Identity>(feature);
                description = $"There's {name.description} here";
            }

            var itemCount = world.spatial.ItemsCountAt(coords);
            if (itemCount == 1)
            {
                var item = world.spatial.GetItemOrThrow(coords);
                var name = Items.GetName(world, item);
                description = $"There's {name} here";
            }
            else if (itemCount > 1)
            {
                description = "There are several items here";
            }

            description ??= cell.Tile.description;

            return description;
        }
    }
}