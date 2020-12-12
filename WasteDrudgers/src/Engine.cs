using System;
using WasteDrudgers.State;

namespace WasteDrudgers
{
    internal class Engine
    {
        private const int TARGET_FPS = 60;
        private const float SECONDS_PER_FRAME = 1f / TARGET_FPS;

        private IEngineContext ctx;
        private World world;

        private Engine()
        {
            Console.WriteLine("Initializing engine...");

            world = new World();
            ctx = new Context(world);

            world.SetState(ctx, RunState.MainMenu(0));
        }

        private static void Main(string[] args)
        {
            var engine = new Engine();
            while (true)
            {
                engine.ctx.IncrementDeltaTime();
                var state = engine.world.State;
                if (state == null)
                {
                    break;
                }

                // TODO: Queue probably should be a part of the Context, 
                // as it's really not part of the game content, but inputs 
                if (engine.world.repeatActionQueue.Count == 0)
                {
                    engine.ctx.HandleInput(state.InputDomains);
                }
                engine.world.Tick(engine.ctx);
                engine.ctx.Render();

                if (engine.ctx.DeltaTime < SECONDS_PER_FRAME)
                {
                    engine.ctx.WaitNextFrame(SECONDS_PER_FRAME - engine.ctx.DeltaTime);
                }
            }

            Console.WriteLine("Disposing engine resources...");
            engine.ctx.Dispose();

            Console.WriteLine("Terminating program!");
        }
    }
}
