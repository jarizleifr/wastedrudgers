using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void InitiativeSystem(IContext ctx, World world)
        {
            var playerData = world.ecs.FetchResource<PlayerData>();

            // TODO: Is this check actually needed? I don't think we ever end up here with a Turn component anyway?
            if (!world.ecs.Has<Turn>(playerData.entity))
            {
                bool playerReady = false;
                if (world.queue.Empty)
                {
                    world.ecs.Loop<Actor>((Entity entity, ref Actor actor) =>
                    {
                        actor.energy += actor.speed;

                        if (actor.energy >= 1000)
                        {
                            if (entity == playerData.entity)
                            {
                                world.ecs.Assign(entity, new Turn { });
                                playerReady = true;
                            }
                            else
                            {
                                world.queue.Add(entity);
                            }
                        }
                    });
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