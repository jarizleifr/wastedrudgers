using ManulECS;
using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void BumpSystem(IContext ctx, World world)
        {
            world.ecs.Loop<Position, Actor, IntentionMove>((Entity entity, ref Position pos, ref Actor actor, ref IntentionMove move) =>
            {
                var tryPos = pos.coords + move.transform;

                if (world.spatial.TryGetCreature(tryPos, out var creature))
                {
                    actor.energy -= 1000;
                    world.ecs.Remove<Turn>(entity);
                    world.ecs.Remove<IntentionMove>(entity);

                    world.ecs.Assign(entity, new IntentionAttack { attacker = entity, target = creature });
                }
            });
        }
    }
}