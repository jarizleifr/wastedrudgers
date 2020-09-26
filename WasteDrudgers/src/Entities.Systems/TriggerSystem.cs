using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void TriggerSystem(IContext ctx, World world)
        {
            world.ecs.Loop<Position, EventMoved>((Entity entity, ref Position pos, ref EventMoved moved) =>
            {
                if (world.spatial.TryGetFeature(pos.coords, out var feature))
                {
                    if (world.ecs.TryGet(feature, out EntryTrigger trigger))
                    {
                        Features.Trigger(world, entity, feature, pos.coords, trigger);
                    }
                }
            });
        }
    }
}