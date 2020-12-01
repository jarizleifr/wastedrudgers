using System.Collections.Generic;
using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers.State
{
    internal class LevelGenerationState : IRunState
    {
        public string levelName;
        public Map map = null;
        public bool newGame;

        public void Run(IContext ctx, World world)
        {
            bool initial = map == null;

            var level = Data.GetLevel(levelName);
            level.Strategy.Generate(world, levelName, ref map);

            world.Map = map;
            world.spatial.Populate(world);

            CreatePortals(world, map, level.Portals);
            TryLevelTransition(world);

            if (level.Items != null || level.Monsters != null)
            {
                var spawner = new EntitySpawner(ctx, world, level);
                if (initial)
                {
                    spawner.InitialSpawning(world, level.Items);
                }
                world.ecs.SetResource(spawner);
            }
            CreateDecorations(world, map);

            if (newGame)
            {
                var playerData = world.PlayerData;
                var pos = LevelUtils.GetRandomPassableEmptyPosition(world);
                Features.CreateFeature(world, pos, "start_portal");
                world.ecs.AssignOrReplace(playerData.entity, new Position { coords = pos });
            }

            RNG.Seed();

            world.SetState(ctx, RunState.Ticking);
        }

        private void CreateDecorations(World world, Map map)
        {
            var decorationCount = RNG.Int(50);
            for (int i = 0; i < decorationCount; i++)
            {
                var pos = LevelUtils.GetRandomPassablePositionWithoutFeature(world);

                Features.CreateFeature(world, pos, RNG.Int(8) switch
                {
                    0 => "vegetation",
                    1 => "thorns",
                    2 => "gravestone",
                    3 => "reeds",
                    4 => "rubble_01",
                    5 => "rubble_02",
                    6 => "altar",
                    7 => "mushroom_patch",
                });
            }
        }

        private void CreatePortals(World world, Map map, List<DBPortal> portals)
        {
            if (portals == null) return;

            foreach (var portal in portals)
            {
                var pos = LevelUtils.GetRandomPassablePositionWithoutFeature(world);
                Features.CreatePortal(world, pos, portal);
            }
        }

        private void TryLevelTransition(World world)
        {
            var playerData = world.PlayerData;
            if (playerData.currentLevel != levelName)
            {
                foreach (var e in world.ecs.View<Position, Portal>())
                {
                    ref var pos = ref world.ecs.GetRef<Position>(e);
                    ref var portal = ref world.ecs.GetRef<Portal>(e);

                    if (portal.targetLevel == playerData.currentLevel)
                    {
                        world.ecs.AssignOrReplace(playerData.entity, pos);
                        playerData.coords = pos.coords;
                        break;
                    }
                }
                playerData.currentLevel = levelName;
            }
        }
    }
}