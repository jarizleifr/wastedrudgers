namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void ItemSystem(IContext ctx, World world)
        {
            var positions = world.ecs.Pools<Position>();
            foreach (var e in world.ecs.View<Position, Actor, IntentionGetItem>())
            {
                ref var pos = ref positions[e];

                if (world.spatial.ItemsCountAt(pos.coords) > 0)
                {
                    var item = world.spatial.GetItemOrThrow(pos.coords);
                    world.WriteToLog("get_item", pos.coords, LogArgs.Actor(e), LogArgs.Item(item));
                    Items.PickUpItem(world, e, item);
                }
                world.ecs.Assign<EventActed>(e, new EventActed { energyLoss = 1000, nutritionLoss = 2 });
            }

            var intents = world.ecs.Pools<IntentionUseItem>();
            foreach (var e in world.ecs.View<Position, Actor, IntentionUseItem>())
            {
                ref var intent = ref intents[e];

                Items.UseItem(world, e, intent.item);
                world.ecs.Assign<EventActed>(e, new EventActed { energyLoss = 1000, nutritionLoss = 2 });
            }
        }
    }
}