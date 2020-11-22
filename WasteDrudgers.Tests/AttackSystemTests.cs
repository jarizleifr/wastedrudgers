using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Common;
using WasteDrudgers.Entities;
using Xunit;

namespace WasteDrudgers.Tests
{
    public class AttackSystemTests
    {
        private World world;
        private IEngineContext ctx;

        public AttackSystemTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void Target_WontReceiveDamage_FromMissedAttack()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "mutorc", new Vec2(0, 2));
            var e2 = Creatures.Create(world, "mutorc", new Vec2(0, 3));

            var startVigor = world.ecs.GetRef<Health>(e2).vigor.Current;

            world.ecs.AssignOrReplace(e1, new Combat
            {
                hitChance = 0,
                damage = new Extent(5, 5),
            });
            world.ecs.Assign(e1, new IntentionAttack { target = e2 });

            Systems.AttackSystem(ctx, world);

            Assert.Equal(0, world.ecs.Count<Damage>());
        }

        [Fact]
        public void Target_ReceivesDamage_FromAttack()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "mutorc", new Vec2(0, 2));
            var e2 = Creatures.Create(world, "mutorc", new Vec2(0, 3));

            var startVigor = world.ecs.GetRef<Health>(e2).vigor.Current;

            world.ecs.AssignOrReplace(e1, new Combat
            {
                hitChance = 100,
                damage = new Extent(5, 5)
            });
            world.ecs.Assign(e1, new IntentionAttack { target = e2 });

            Systems.AttackSystem(ctx, world);

            int i = 0;
            foreach (var e in world.ecs.View<Damage>())
            {
                ref var d = ref world.ecs.GetRef<Damage>(e);
                Assert.Equal(e2, d.target);
                i++;
            }
            Assert.Equal(1, i);
        }

        [Fact]
        public void Target_ReceivesActiveEffect_FromPoisonedAttack()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "fungoid", new Vec2(0, 2));
            var e2 = Creatures.Create(world, "mutorc", new Vec2(0, 3));

            world.ecs.AssignOrReplace(e1, new Combat
            {
                hitChance = 100,
                damage = new Extent(1, 1)
            });
            world.ecs.Assign(e1, new IntentionAttack { target = e2 });

            Systems.AttackSystem(ctx, world);

            int i = 0;
            foreach (var e in world.ecs.View<ActiveEffect>())
            {
                i++;
            }
            Assert.Equal(1, i);
        }

        [Fact]
        public void Damage_HasPlayerInitiated_WhenPlayerInitiated()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "mutorc", new Vec2(0, 2));

            var player = world.PlayerData.entity;
            world.ecs.AssignOrReplace(player, new Combat
            {
                hitChance = 100,
                damage = new Extent(1, 1)
            });
            world.ecs.Assign(player, new IntentionAttack { target = e1 });
            Systems.AttackSystem(ctx, world);

            int i = 0;
            foreach (var e in world.ecs.View<Damage, PlayerInitiated>())
            {
                ref var d = ref world.ecs.GetRef<Damage>(e);
                Assert.Equal(e1, d.target);
                i++;
            }
            Assert.Equal(1, i);
        }
    }
}