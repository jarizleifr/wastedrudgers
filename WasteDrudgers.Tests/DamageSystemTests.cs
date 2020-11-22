using Blaggard.Common;
using WasteDrudgers.Entities;
using Xunit;

namespace WasteDrudgers.Tests
{
    public class DamageSystemTests
    {
        private World world;
        private IEngineContext ctx;

        public DamageSystemTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void Player_ReceivesExperience_FromInitiatedKill()
        {
            var player = world.PlayerData.entity;
            var exp = world.ecs.GetRef<Experience>(player);

            RNG.Seed(0);
            var e1 = Creatures.Create(world, "mutorc", new Vec2(0, 2));

            var damageEntity = world.ecs.Create();
            world.ecs.Assign(damageEntity, new Damage { damage = 1000, target = e1 });
            world.ecs.Assign(damageEntity, new PlayerInitiated { });

            Systems.DamageSystem(ctx, world);
            var newExp = world.ecs.GetRef<Experience>(player);
            Assert.True(newExp.experience > exp.experience);
        }

        [Fact]
        public void Creature_Dies_FromFatalDamage()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "mutorc", new Vec2(0, 2));
            world.ecs.Assign(world.ecs.Create(), new Damage { damage = 1000, target = e1 });

            Systems.DamageSystem(ctx, world);
            Assert.True(world.ecs.Has<Death>(e1));
        }
    }
}