using Xunit;
using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Entities;

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
    }
}