using WasteDrudgers.State;
using Xunit;

namespace WasteDrudgers.Tests
{
    public class PlayerTests
    {
        private World world;
        private IEngineContext ctx;

        public PlayerTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void PlayerGains15CharacterPoints_UponHittingExperienceThreshold()
        {
            var player = world.PlayerData.entity;
            world.ecs.Assign(player, new Turn { });
            world.ecs.AssignOrReplace(player, new Experience { level = 1, experience = 1000 });
            world.SetState(ctx, RunState.Ticking);
            world.Tick(ctx);

            var exp = world.ecs.GetRef<Experience>(player);
            Assert.Equal(15, exp.characterPoints);
        }
    }
}
