namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void HungerClockSystem(IContext ctx, World world)
        {
            var playerData = world.PlayerData;

            // TODO: Still wondering if HungerClock should be a Resource instead of component
            var (clocks, pools) = world.ecs.Pools<HungerClock, Pools>();
            foreach (var e in world.ecs.View<HungerClock, EventInventoryUpdated>())
            {
                ref var clock = ref clocks[e];
                var state = clock.State;
                clock.food = Items.GetRations(world, e);
                if (clock.State != state)
                {
                    world.ecs.Assign<EventStatsUpdated>(e);
                }
            }

            foreach (var e in world.ecs.View<HungerClock, Pools>())
            {
                ref var clock = ref clocks[e];
                ref var health = ref pools[e];

                var state = clock.State;
                if (clock.nutrition == 0)
                {
                    if (clock.food > 0)
                    {
                        (var gotNutrition, var rationsRemaining) = Items.TryAutoConsume(world, e);
                        clock.nutrition = gotNutrition;
                        clock.food = rationsRemaining;
                    }
                    else
                    {
                        if (!health.fatigued)
                        {
                            health.fatigued = true;
                        }
                    }
                }
                if (clock.State != state)
                {
                    world.ecs.Assign<EventStatsUpdated>(e);
                }
            }

            if (world.ecs.Has<EventInventoryUpdated>(playerData.entity))
            {
                Items.UpdateFoodLeft(world);
            }
        }
    }
}