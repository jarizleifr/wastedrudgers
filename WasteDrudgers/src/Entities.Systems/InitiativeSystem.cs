namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void InitiativeSystem(IContext ctx, World world)
        {
            var playerData = world.PlayerData;

            bool playerReady = false;
            if (world.queue.Empty)
            {
                var actors = world.ecs.Pools<Actor>();
                foreach (var e in world.ecs.View<Actor>())
                {
                    ref var actor = ref actors[e];
                    actor.energy += actor.speed;

                    if (actor.energy >= 1000)
                    {
                        if (e == playerData.entity)
                        {
                            world.ecs.Assign<Turn>(e);
                            playerReady = true;
                        }
                        else
                        {
                            world.queue.Add(e);
                        }
                    }
                }
            }

            if (!playerReady)
            {
                foreach (var e in world.queue.Entities)
                {
                    world.ecs.Assign<Turn>(e);
                }
                world.queue.Clear();
            }
        }
    }
}