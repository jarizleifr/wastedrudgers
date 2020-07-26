using Blaggard.Graphics;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    public class MoreMessagesState : IRunState
    {
        public string[] InputDomains { get; set; } = { "game" };

        public void Run(IContext ctx, World world)
        {
            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.Print(root.Width - 1, 2, "-MORE-", world.database.GetColor("c_white"), TextAlignment.Right);

            if (ctx.Command != Command.None)
            {
                world.ShouldRedraw = true;
                world.log.UpdateMessageBuffer();
            }

            if (!world.log.HasMessages())
            {
                world.SetState(ctx, RunState.AwaitingInput);
            }

            Views.DrawGameView(ctx, world);
        }
    }
}