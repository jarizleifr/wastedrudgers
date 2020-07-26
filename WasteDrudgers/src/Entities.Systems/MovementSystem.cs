using ManulECS;
using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        // TODO: Make sure player moves always first if its their turn
        public static void MovementSystem(IContext ctx, World world)
        {
            var map = world.ecs.FetchResource<Map>();
            world.ecs.Loop<Position, Actor, IntentionMove>((Entity entity, ref Position pos, ref Actor actor, ref IntentionMove move) =>
            {
                var tryPos = pos.coords + move.transform;

                if (!map[tryPos].Flags(TileFlags.BlocksMovement))
                {
                    if (world.spatial.TryMoveCreature(entity, pos.coords, tryPos))
                    {
                        pos.coords = tryPos;
                        world.ecs.Assign(entity, new EventMoved { });
                    }
                    // TODO: Bump checking should be here instead of in its own system
                }
                else
                {
                    if (world.ecs.Has<Player>(entity))
                    {
                        world.WriteToLog("crashed_to_wall", tryPos);
                    }
                }
                actor.energy -= 1000;
                world.ecs.Remove<Turn>(entity);
                world.ecs.Remove<IntentionMove>(entity);
            });

            // Update cached player position
            var playerData = world.ecs.FetchResource<PlayerData>();
            if (world.ecs.TryGet<Position>(playerData.entity, out var pos))
            {
                playerData.coords = pos.coords;
            }
        }
    }
}