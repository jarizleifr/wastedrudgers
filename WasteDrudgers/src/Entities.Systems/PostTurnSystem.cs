namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void PostTurnSystem(IContext ctx, World world)
        {
            foreach (var e in world.ecs.View<Actor, EventActed>())
            {
                ref var actor = ref world.ecs.GetRef<Actor>(e);
                ref var ev = ref world.ecs.GetRef<EventActed>(e);
                actor.energy -= ev.energyLoss;
            }

            foreach (var e in world.ecs.View<HungerClock, EventActed>())
            {
                ref var clock = ref world.ecs.GetRef<HungerClock>(e);
                ref var ev = ref world.ecs.GetRef<EventActed>(e);

                var state = clock.State;
                clock.nutrition -= ev.nutritionLoss;
                if (clock.nutrition < 0)
                {
                    clock.nutrition = 0;
                }

                if (state != clock.State)
                {
                    world.ecs.Assign(e, new EventStatusUpdated { });
                }
            }
        }
    }
}
