using Blaggard.Common;
using ManulECS;
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
            var e1 = Creatures.Create(world, "cr_mutorc", new Vec2(0, 2));

            var playerData = world.ecs.FetchResource<PlayerData>();
            Spells.CastSpellOn(world, playerData.entity, e1, "spl_poison_weak");

            int i = 0;
            world.ecs.Loop((Entity entity, ref ActiveEffect a, ref PlayerInitiated p) =>
            {
                Assert.Equal(e1, a.target);
                i++;
            });
            Assert.Equal(1, i);
        }

        [Fact]
        public void IdentifySpell_IdentifiesInventory()
        {
            RNG.Seed(0);
            var e1 = Items.Create(world, "itm_potion_health", new Vec2(0, 2));
            var e2 = Items.Create(world, "itm_potion_vigor", new Vec2(0, 2));

            var playerData = world.ecs.FetchResource<PlayerData>();
            Items.PickUpItem(world, playerData.entity, e1);
            Items.PickUpItem(world, playerData.entity, e2);
            Spells.CastSpellOn(world, null, playerData.entity, "spl_identify");

            var item1 = world.ecs.GetRef<Item>(e1);
            var item2 = world.ecs.GetRef<Item>(e2);

            Assert.Equal(IdentificationStatus.Identified, item1.status);
            Assert.Equal(IdentificationStatus.Identified, item2.status);
        }
    }
}