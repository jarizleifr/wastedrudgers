using ManulECS;
using WasteDrudgers.Level;

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

                actor.energy -= 1000;
                world.ecs.Remove<Turn>(entity);
                world.ecs.Remove<IntentionGetItem>(entity);
            });

            world.ecs.Loop((Entity entity, ref Position pos, ref Actor actor, ref IntentionUseItem intent) =>
            {
                Items.UseItem(world, entity, intent.item);

                actor.energy -= 1000;
                world.ecs.Remove<Turn>(entity);
                world.ecs.Remove<IntentionUseItem>(entity);
            });
        }
    }
}