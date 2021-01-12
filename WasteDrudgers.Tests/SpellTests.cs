using Blaggard.Common;
using WasteDrudgers.Entities;
using Xunit;

namespace WasteDrudgers.Tests
{
    public class SpellTests
    {
        private World world;
        private IEngineContext ctx;

        public SpellTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void ActiveEffect_HasPlayerInitiated_WhenPlayerInitiated()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "mutorc", new Vec2(0, 2));

            var playerData = world.PlayerData;
            Effects.CastSpell(world, e1, "poison_weak", playerData.entity);

            int i = 0;
            foreach (var e in world.ecs.View<ActiveEffect, PlayerInitiated>())
            {
                ref var a = ref world.ecs.GetRef<ActiveEffect>(e);
                Assert.Equal(e1, a.target);
                i++;
            }
            Assert.Equal(1, i);
        }

        [Fact]
        public void IdentifySpell_IdentifiesInventory()
        {
            RNG.Seed(0);
            var e1 = Items.Create(world, "potion_health", new Vec2(0, 2));
            var e2 = Items.Create(world, "potion_vigor", new Vec2(0, 2));

            var playerData = world.PlayerData;
            Items.PickUpItem(world, playerData.entity, e1);
            Items.PickUpItem(world, playerData.entity, e2);
            Effects.CastSpell(world, playerData.entity, "identify", null);

            var item1 = world.ecs.GetRef<Item>(e1);
            var item2 = world.ecs.GetRef<Item>(e2);

            Assert.Equal(IdentificationStatus.Identified, item1.status);
            Assert.Equal(IdentificationStatus.Identified, item2.status);
        }
    }
}