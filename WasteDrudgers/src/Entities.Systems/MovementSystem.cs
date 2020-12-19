using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        // TODO: Make sure player moves always first if its their turn
        public static void MovementSystem(IContext ctx, World world)
        {
            var map = world.Map;
            var (positions, moves) = world.ecs.Pools<Position, IntentionMove>();
            foreach (var e in world.ecs.View<Position, Actor, IntentionMove>())
            {
                ref var pos = ref positions[e];
                ref var move = ref moves[e];

                var tryPos = pos.coords + move.transform;

                if (!map[tryPos].Flags(TileFlags.BlocksMovement))
                {
                    if (world.spatial.TryMoveCreature(e, pos.coords, tryPos))
                    {
                        pos.coords = tryPos;
                        world.ecs.Assign<EventMoved>(e);
                    }
                    else
                    {
                        // Bumped into someone
                        if (world.spatial.TryGetCreature(tryPos, out var creature))
                        {
                            world.ecs.Assign(e, new IntentionAttack { attacker = e, target = creature });
                            continue;
                        }
                    }
                }
                else
                {
                    if (world.ecs.Has<Player>(e))
                    {
                        world.WriteToLog("crashed_to_wall", tryPos);
                    }
                }
                world.ecs.Assign<EventActed>(e, new EventActed { energyLoss = 1000, nutritionLoss = 2 });
            }

            // Update cached player position
            var playerData = world.PlayerData;
            if (world.ecs.TryGet<Position>(playerData.entity, out var playerPos))
            {
                playerData.coords = playerPos.coords;
            }
        }
    }
}