namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void ItemSystem(IContext ctx, World world)
        {
            foreach (var e in world.ecs.View<Position, Actor, IntentionGetItem>())
            {
                ref var pos = ref world.ecs.GetRef<Position>(e);
                ref var actor = ref world.ecs.GetRef<Actor>(e);
                ref var get = ref world.ecs.GetRef<IntentionGetItem>(e);

                if (world.spatial.ItemsCountAt(pos.coords) > 0)
                {
                    var item = world.spatial.GetItemOrThrow(pos.coords);
                    world.WriteToLog("get_item", pos.coords, LogItem.Actor(e), LogItem.Item(item));
                    Items.PickUpItem(world, e, item);
                }
                world.ecs.Assign<EventActed>(e, new EventActed { energyLoss = 1000, nutritionLoss = 2 });
            }

            foreach (var e in world.ecs.View<Position, Actor, IntentionUseItem>())
            {
                ref var pos = ref world.ecs.GetRef<Position>(e);
                ref var actor = ref world.ecs.GetRef<Actor>(e);
                ref var intent = ref world.ecs.GetRef<IntentionUseItem>(e);

                Items.UseItem(world, e, intent.item);
                world.ecs.Assign<EventActed>(e, new EventActed { energyLoss = 1000, nutritionLoss = 2 });
            }
        }
    }
}