namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void StatusSystem(IContext ctx, World world)
        {
            foreach (var e in world.ecs.View<Stats, HungerClock, EventStatusUpdated>())
            {
                ref var stats = ref world.ecs.GetRef<Stats>(e);
                ref var clock = ref world.ecs.GetRef<HungerClock>(e);

                if (clock.State == HungerState.Hungry)
                {
                    stats.SetMods(-2);
                }
                else
                {
                    stats.SetMods(0);
                }
                world.ecs.Assign<EventStatsUpdated>(e);
            }
        }
    }
}