using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Entities;
using Xunit;

namespace WasteDrudgers.Tests
{
    public class FeatureTests
    {
        private World world;
        private IEngineContext ctx;

        public FeatureTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void EntityReceivesDamage_FromThornsTrigger()
        {
            var pos = new Vec2(0, 2);
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "mutorc", pos);
            var f = Features.CreateFeature(world, pos, "thorns");
            var trigger = world.ecs.GetRef<EntryTrigger>(f);

            Features.Trigger(world, e1, f, pos, trigger);

            int i = 0;
            foreach (var e in world.ecs.View<Damage>())
            {
                ref var d = ref world.ecs.GetRef<Damage>(e);
                Assert.Equal(e1, d.target);
                i++;
            }
            Assert.Equal(1, i);
        }
    }
}