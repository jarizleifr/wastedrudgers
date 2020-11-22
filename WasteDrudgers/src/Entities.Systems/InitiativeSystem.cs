namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void InitiativeSystem(IContext ctx, World world)
        {
            var playerData = world.PlayerData;

            // TODO: Is this check actually needed? I don't think we ever end up here with a Turn component anyway?
            if (!world.ecs.Has<Turn>(playerData.entity))
            {
                bool playerReady = false;
                if (world.queue.Empty)
                {
                    foreach (var e in world.ecs.View<Actor>())
                    {
                        ref var actor = ref world.ecs.GetRef<Actor>(e);
                        actor.energy += actor.speed;

                        if (actor.energy >= 1000)
                        {
                            if (e == playerData.entity)
                            {
                                world.ecs.Assign(e, new Turn { });
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
                        world.ecs.Assign(e, new Turn { });
                    }
                    world.queue.Clear();
                }
            }
        }
    }
}