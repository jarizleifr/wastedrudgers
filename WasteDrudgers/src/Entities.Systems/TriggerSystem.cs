namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void TriggerSystem(IContext ctx, World world)
        {
            foreach (var e in world.ecs.View<Position, EventMoved>())
            {
                ref var pos = ref world.ecs.GetRef<Position>(e);

                if (world.spatial.TryGetFeature(pos.coords, out var feature))
                {
                    if (world.ecs.TryGet(feature, out EntryTrigger trigger))
                    {
                        Features.Trigger(world, e, feature, pos.coords, trigger);
                    }
                }
            };
        }
    }
}