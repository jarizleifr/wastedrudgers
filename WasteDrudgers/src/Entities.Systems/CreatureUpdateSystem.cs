namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void CreatureUpdateSystem(IContext ctx, World world)
        {
            foreach (var e in world.ecs.View<EventStatsUpdated>())
            {
                Creatures.UpdateCreature(world, e);
            }
        }
    }
}