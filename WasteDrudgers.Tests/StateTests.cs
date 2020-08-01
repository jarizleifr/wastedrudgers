using Xunit;
using WasteDrudgers.State;

namespace WasteDrudgers.Tests
{
    public class StateTests
    {
        private World world;
        private IEngineContext ctx;

        public StateTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void SetsGameOverState_WhenPlayerIsDead()
        {
            var player = world.ecs.FetchResource<PlayerData>().entity;
            world.ecs.Assign(player, new Death { });
            world.SetState(ctx, RunState.Ticking);
            world.Tick(ctx);

            Assert.True(world.State is GameOverState);
        }
    }
}