using System.IO;
using WasteDrudgers.Render;
using WasteDrudgers.UI;

namespace WasteDrudgers.State
{
    [InputDomains("menu")]
    internal class ManualState : GameScene
    {
        private TextFileReader reader;

        public override void Initialize(IContext ctx, World world)
        {
            reader = new TextFileReader(Path.Combine("assets", "manual.txt"), 23);
        }

        public override void Update(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuUp:
                    reader.ScrollUp();
                    break;
                case Command.MenuDown:
                    reader.ScrollDown();
                    break;

                case Command.Exit:
                    // TODO: If we put manual to main menu as well, 
                    // need some way of determining where to return on exit
                    world.SetState(ctx, RunState.AwaitingInput);
                    break;
            }

            var offsets = RenderUtils.GetTerminalWindowOffsets(ctx);

            var layer = ctx.QueueCanvas(RenderLayer.MenuOverlay);
            var rect = RenderUtils.TerminalWindow(ctx);

            layer.Clear();
            layer.SetRenderPosition(offsets.offsetX, offsets.offsetY);

            layer.DefaultFore = ctx.Theme.windowFrame;
            layer.DefaultBack = ctx.Theme.windowBackground;
            layer.PrintFrame(rect, true);

            reader.Draw(ctx, layer, 1, 1);
        }
    }
}