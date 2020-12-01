using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void SpawnSystem(IContext ctx, World world)
        {
            if (world.GameTicks % 50 == 0)
            {
                var spawner = world.ecs.GetResource<EntitySpawner>();
                if (world.ecs.Count<Actor>() < spawner.MonsterSpawns.min)
                {
                    spawner.Spawn(world, LevelUtils.GetRandomPassablePositionWithoutCreature(world));
                }
            }
        }
    }
}