using System.Collections.Generic;
using ManulECS;
using WasteDrudgers.Data;
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

            var level = world.database.GetLevel(levelName);
            level.Strategy.Generate(world, levelName, ref map);

            world.ecs.SetResource(map);

            world.spatial.Populate(world);
            world.fov.Clear();

            CreatePortals(world, map, level.Portals);
            TryLevelTransition(world);

            if (level.MinSpawn > 0 && level.MaxSpawn > 0)
            {
                var spawner = new EntitySpawner(ctx, world, level);
                if (initial)
                {
                    spawner.InitialSpawning(world);
                }
                world.ecs.SetResource(spawner);
            }
            CreateDecorations(world, map);

            if (newGame)
            {
                var playerData = world.ecs.FetchResource<PlayerData>();
                var pos = LevelUtils.GetRandomPassableEmptyPosition(world);
                Features.CreateFeature(world, pos, "fea_start_portal");
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
                    0 => "fea_vegetation",
                    1 => "fea_thorns",
                    2 => "fea_gravestone",
                    3 => "fea_reeds",
                    4 => "fea_rubble",
                    5 => "fea_rubble2",
                    6 => "fea_altar",
                    7 => "fea_mushroom_patch",
                });
            }
        }

        private void CreatePortals(World world, Map map, List<DBPortal> portals)
        {
            foreach (var portal in portals)
            {
                var pos = LevelUtils.GetRandomPassablePositionWithoutFeature(world);
                Features.CreatePortal(world, pos, portal);
            }
        }

        private void TryLevelTransition(World world)
        {
            var playerData = world.ecs.FetchResource<PlayerData>();
            if (playerData.currentLevel != levelName)
            {
                world.ecs.Loop((Entity entity, ref Position pos, ref Portal portal) =>
                {
                    if (portal.targetLevel == playerData.currentLevel)
                    {
                        world.ecs.AssignOrReplace(playerData.entity, pos);
                        playerData.coords = pos.coords;
                        return;
                    }
                });
                playerData.currentLevel = levelName;
            }
        }
    }
}