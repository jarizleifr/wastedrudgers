namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void PostTurnSystem(IContext ctx, World world)
        {
            var (actors, actedEvents) = world.ecs.Pools<Actor, EventActed>();
            foreach (var e in world.ecs.View<Actor, EventActed>())
            {
                ref var actor = ref actors[e];
                ref var ev = ref actedEvents[e];
                actor.energy -= ev.energyLoss;
            }

            var clocks = world.ecs.Pools<HungerClock>();
            foreach (var e in world.ecs.View<HungerClock, EventActed>())
            {
                ref var clock = ref clocks[e];
                ref var ev = ref actedEvents[e];

                var state = clock.State;
                clock.nutrition -= ev.nutritionLoss;
                if (clock.nutrition < 0)
                {
                    clock.nutrition = 0;
                }

                if (state != clock.State)
                {
                    world.ecs.Assign<EventStatusUpdated>(e);
                }
            }
        }
    }
}
