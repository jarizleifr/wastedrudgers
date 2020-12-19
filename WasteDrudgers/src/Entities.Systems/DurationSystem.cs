namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void DurationSystem(IContext ctx, World world)
        {
            if (world.GameTicks % 10 == 0)
            {
                var durations = world.ecs.Pools<Duration>();
                foreach (var e in world.ecs.View<Duration>())
                {
                    ref var dur = ref durations[e];
                    dur.duration--;

                    if (dur.duration < 0)
                    {
                        if (world.ecs.TryGet<ActiveEffect>(e, out var effect))
                        {
                            world.ecs.Assign<EventStatsUpdated>(effect.target);
                        }
                        world.ecs.Remove(e);
                    }
                }
            }
        }
    }
}