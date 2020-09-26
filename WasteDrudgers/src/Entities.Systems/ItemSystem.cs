using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void ItemSystem(IContext ctx, World world)
        {
            world.ecs.Loop((Entity entity, ref Position pos, ref Actor actor, ref IntentionGetItem get) =>
            {
                if (world.spatial.ItemsCountAt(pos.coords) > 0)
                {
                    var item = world.spatial.GetItemOrThrow(pos.coords);
                    world.WriteToLog("get_item", pos.coords, LogItem.Actor(entity), LogItem.Item(item));
                    Items.PickUpItem(world, entity, item);
                }
                world.ecs.Assign<EventActed>(entity, new EventActed { energyLoss = 1000, nutritionLoss = 2 });
            });

            world.ecs.Loop((Entity entity, ref Position pos, ref Actor actor, ref IntentionUseItem intent) =>
            {
                Items.UseItem(world, entity, intent.item);
                world.ecs.Assign<EventActed>(entity, new EventActed { energyLoss = 1000, nutritionLoss = 2 });
            });
        }
    }
}