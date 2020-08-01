using Xunit;
using ManulECS;
using Blaggard.Common;
using WasteDrudgers.Entities;

namespace WasteDrudgers.Tests
{
    public class InventoryTests
    {
        private World world;
        private IEngineContext ctx;

        public InventoryTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void PickingUpItems_IncrementsSameItemCount()
        {
            var item1 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 0));
            var item2 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 1));
            var item3 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 2));

            var player = world.ecs.FetchResource<PlayerData>().entity;

            Items.PickUpItem(world, player, item1);
            Items.PickUpItem(world, player, item2);
            Items.PickUpItem(world, player, item3);

            int count = 0;
            world.ecs.Loop<InBackpack, Item>((Entity entity, ref InBackpack b, ref Item item) =>
            {
                count++;
                Assert.Equal(3, item.count);
            });
            Assert.Equal(1, count);
        }

        [Fact]
        public void UnequippingItems_IncrementsSameItemCount()
        {
            var item1 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 0));
            var item2 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 1));
            var item3 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 2));

            Items.IdentifyItem(world, item1);
            Items.IdentifyItem(world, item2);
            Items.IdentifyItem(world, item3);

            var player = world.ecs.FetchResource<PlayerData>().entity;

            Items.PickUpItem(world, player, item1);
            Items.EquipItem(world, player, item1);

            Items.PickUpItem(world, player, item2);
            Items.PickUpItem(world, player, item3);

            Items.UnequipItemToBackpack(world, player, Slot.MainHand);

            int count = 0;
            world.ecs.Loop<InBackpack, Item>((Entity entity, ref InBackpack b, ref Item item) =>
            {
                count++;
                Assert.Equal(3, item.count);
            });
            Assert.Equal(1, count);
        }

        [Fact]
        public void EquippingItems_DecrementsSameItemCount()
        {
            var item1 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 0));
            var item2 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 1));
            var item3 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 2));

            Items.IdentifyItem(world, item1);
            Items.IdentifyItem(world, item2);
            Items.IdentifyItem(world, item3);

            var player = world.ecs.FetchResource<PlayerData>().entity;

            Items.PickUpItem(world, player, item1);
            Items.PickUpItem(world, player, item2);
            Items.PickUpItem(world, player, item3);

            Items.EquipItem(world, player, item1);

            int count = 0;
            world.ecs.Loop<InBackpack, Item>((Entity entity, ref InBackpack b, ref Item item) =>
            {
                count++;
                Assert.Equal(2, item.count);
            });
            Assert.Equal(1, count);
        }

        [Fact]
        public void SameItemsFound_AfterSwitchingEquipment()
        {
            var item1 = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 0));
            var dg = Items.CreateItem(world, "itm_dagger", "mat_iron", new Vec2(0, 0));
            var item2 = Items.CreateItem(world, "itm_longsword", "mat_iron", new Vec2(0, 1));
            var item3 = Items.CreateItem(world, "itm_shortsword", "mat_iron", new Vec2(0, 2));

            Items.IdentifyItem(world, item1);
            Items.IdentifyItem(world, item2);
            Items.IdentifyItem(world, item3);

            var player = world.ecs.FetchResource<PlayerData>().entity;

            Items.PickUpItem(world, player, item1);
            Items.PickUpItem(world, player, item2);
            Items.PickUpItem(world, player, item3);

            Items.EquipItem(world, player, item1);
            Items.EquipItem(world, player, item2);
            Items.EquipItem(world, player, item3);

            Items.UnequipItemToBackpack(world, player, Slot.MainHand);

            int count = 0;
            world.ecs.Loop<InBackpack, Item, Identity>((Entity entity, ref InBackpack b, ref Item item, ref Identity id) =>
            {
                count++;
                if (id.rawName == "itm_dagger")
                {
                    Assert.Equal(2, item.count);
                }
            });
            Assert.Equal(3, count);
        }
    }
}
