using System.Collections.Generic;
using WasteDrudgers.Entities;
using WasteDrudgers.State;

namespace WasteDrudgers.Tests
{
    public static class TestUtils
    {
        public static (World world, IEngineContext ctx) CreateGame()
        {
            var world = new World();
            var ctx = new TestContext();

            RNG.Seed(0);
            world.SetState(ctx, RunState.NewGame(new Skills { set = new List<Skill>() }));
            world.Tick(ctx);
            world.SetState(ctx, RunState.LevelGeneration("lvl_test_arena", null, true));
            world.Tick(ctx);
            world.Tick(ctx);

            return (world, ctx);
        }
    }
}