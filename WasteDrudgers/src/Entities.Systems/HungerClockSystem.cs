namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void HungerClockSystem(IContext ctx, World world)
        {
            var playerData = world.PlayerData;

            foreach (var e in world.ecs.View<HungerClock, EventInventoryUpdated>())
            {
                ref var clock = ref world.ecs.GetRef<HungerClock>(e);
                var state = clock.State;
                clock.food = Items.GetRations(world, e);
                if (clock.State != state)
                {
                    world.ecs.Assign(e, new EventStatusUpdated { });
                }
            }

            foreach (var e in world.ecs.View<HungerClock, Health>())
            {
                ref var clock = ref world.ecs.GetRef<HungerClock>(e);
                ref var health = ref world.ecs.GetRef<Health>(e);

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
                    world.ecs.Assign(e, new EventStatusUpdated { });
                }
            }

            if (world.ecs.Has<EventInventoryUpdated>(playerData.entity))
            {
                Items.UpdateFoodLeft(world);
            }
        }
    }
}