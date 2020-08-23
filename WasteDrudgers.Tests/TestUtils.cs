using WasteDrudgers.State;

namespace WasteDrudgers.Tests
{
    public static class TestUtils
    {
        public static (World world, IEngineContext ctx) CreateGame()
        {
            var world = new World();
            var ctx = new TestContext(world);

            RNG.Seed(0);
            world.SetState(ctx, RunState.NewGame);
            world.Tick(ctx);
            world.SetState(ctx, RunState.LevelGeneration("lvl_test_arena", null, true));
            world.Tick(ctx);
            world.Tick(ctx);

            return (world, ctx);
        }
    }
}