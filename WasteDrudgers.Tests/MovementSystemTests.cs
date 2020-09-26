using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Entities;
using Xunit;

namespace WasteDrudgers.Tests
{
    public class MovementSystemTests
    {
        private World world;
        private IEngineContext ctx;

        public MovementSystemTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void MovesCreatureWith_IntentionMove()
        {
            RNG.Seed(0);
            var e = Creatures.Create(world, "cr_mutorc", new Vec2(0, 2));

            world.ecs.Assign(e, new IntentionMove { transform = new Vec2(1, 1) });
            Systems.MovementSystem(ctx, world);

            var position = world.ecs.GetRef<Position>(e);
            Assert.Equal(new Vec2(1, 3), position.coords);
        }

        [Fact]
        public void HasEventActed_WhenCreatureMoves()
        {
            RNG.Seed(0);
            var e = Creatures.Create(world, "cr_mutorc", new Vec2(0, 2));

            world.ecs.AssignOrReplace(e, new Actor { energy = 1000 });
            world.ecs.Assign(e, new Turn { });
            world.ecs.Assign(e, new IntentionMove { transform = new Vec2(1, 1) });
            Systems.MovementSystem(ctx, world);

            Assert.True(world.ecs.Has<EventActed>(e));
        }

        [Fact]
        public void AssignsIntentionAttack_WhenCreatureBumps()
        {
            RNG.Seed(0);
            var e1 = Creatures.Create(world, "cr_mutorc", new Vec2(0, 2));
            var e2 = Creatures.Create(world, "cr_mutorc", new Vec2(1, 3));

            world.ecs.Assign(e1, new Turn { });
            world.ecs.Assign(e1, new IntentionMove { transform = new Vec2(1, 1) });
            Systems.MovementSystem(ctx, world);

            Assert.True(world.ecs.Has<IntentionAttack>(e1));
        }
    }
}