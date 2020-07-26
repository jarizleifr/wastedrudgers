using System;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    public class CharacterSheetState : IRunState
    {
        public string[] InputDomains { get; set; } = { "menu" };

        public void Initialize(IContext ctx, World world) { }

        public void Run(IContext ctx, World world)
        {
            var menu = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            var rect = RenderUtils.TerminalWindow(ctx);

            switch (ctx.Command)
            {
                case Command.Exit:
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            Views.DrawGameView(ctx, world);
            CharacterUI.DrawCharacterSheet(ctx, world);
        }
    }
}