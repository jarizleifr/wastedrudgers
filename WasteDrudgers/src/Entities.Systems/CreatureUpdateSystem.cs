using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void CreatureUpdateSystem(IContext ctx, World world)
        {
            world.ecs.Loop<EventStatsUpdated>((Entity entity, ref EventStatsUpdated statsUpdated) =>
            {
                Creatures.UpdateCreature(world, entity);
            });
        }
    }
}