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

            var startVigor = world.ecs.GetRef<Pools>(e2).vigor.Current;

            world.ecs.AssignOrReplace(e1, new Attack
            {
                hitChance = 0,
                minDamage = 5,
                maxDamage = 5,
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

            var startVigor = world.ecs.GetRef<Pools>(e2).vigor.Current;

            world.ecs.AssignOrReplace(e1, new Attack
            {
                hitChance = 100,
                minDamage = 5,
                maxDamage = 5,
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
        public void DefendersParryDecreases_WhenAttacked()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "mutorc", new Vec2(0, 2));
            var e2 = Creatures.Create(world, "mutorc", new Vec2(0, 3));

            var startVigor = world.ecs.GetRef<Pools>(e2).vigor.Current;

            world.ecs.AssignOrReplace(e1, new Attack
            {
                hitChance = 100,
                minDamage = 5,
                maxDamage = 5,
            });
            world.ecs.AssignOrReplace(e2, new Defense
            {
                parry = 80,
            });
            world.ecs.Assign(e1, new IntentionAttack { target = e2 });

            Systems.AttackSystem(ctx, world);

            var def = world.ecs.GetRef<Defense>(e2);
            Assert.Equal(80, def.parry);
        }

        [Fact]
        public void Target_ReceivesAffliction_FromPoisonedAttack()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "fungoid", new Vec2(0, 2));
            var e2 = Creatures.Create(world, "mutorc", new Vec2(0, 3));

            world.ecs.AssignOrReplace(e1, new Attack
            {
                hitChance = 100,
                minDamage = 1,
                maxDamage = 1,
            });
            world.ecs.Assign(e1, new IntentionAttack { target = e2 });

            Systems.AttackSystem(ctx, world);

            int i = 0;
            foreach (var e in world.ecs.View<Afflictions>())
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
            world.ecs.AssignOrReplace(e1, new Attack
            {
                hitChance = 100,
                minDamage = 1,
                maxDamage = 1,
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