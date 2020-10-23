using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void HungerClockSystem(IContext ctx, World world)
        {
            var playerData = world.PlayerData;

            world.ecs.Loop((Entity entity, ref HungerClock clock, ref EventInventoryUpdated inventoryUpdated) =>
            {
                var state = clock.State;
                clock.food = Items.GetRations(world, entity);
                if (clock.State != state)
                {
                    world.ecs.Assign(entity, new EventStatusUpdated { });
                }
            });

            world.ecs.Loop((Entity entity, ref HungerClock clock, ref Health health) =>
            {
                var state = clock.State;
                if (clock.nutrition == 0)
                {
                    if (clock.food > 0)
                    {
                        (var gotNutrition, var rationsRemaining) = Items.TryAutoConsume(world, entity);
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
                    world.ecs.Assign(entity, new EventStatusUpdated { });
                }
            });

            if (world.ecs.Has<EventInventoryUpdated>(playerData.entity))
            {
                Items.UpdateFoodLeft(world);
            }
        }
    }
}