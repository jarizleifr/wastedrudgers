using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void PostTurnSystem(IContext ctx, World world)
        {
            world.ecs.Loop<Actor, EventActed>((Entity entity, ref Actor actor, ref EventActed ev) =>
            {
                actor.energy -= ev.energyLoss;
            });

            world.ecs.Loop<HungerClock, EventActed>((Entity entity, ref HungerClock clock, ref EventActed ev) =>
            {
                var state = clock.State;
                clock.nutrition -= ev.nutritionLoss;
                if (clock.nutrition < 0)
                {
                    clock.nutrition = 0;
                }
                
                if (state != clock.State)
                {
                    world.ecs.Assign(entity, new EventStatusUpdated { });
                }
            });
        }
    }
}
