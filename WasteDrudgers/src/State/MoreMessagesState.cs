using Blaggard.Graphics;

namespace WasteDrudgers.State
{
    [InputDomains("game")]
    public class MoreMessagesState : GameScene
    {
        public override void Update(IContext ctx, World world)
        {
            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.Print(root.Width - 1, 2, "-MORE-", Data.Colors.white, TextAlignment.Right);

            if (ctx.Command != Command.None)
            {
                world.ShouldRedraw = true;
                world.log.UpdateMessageBuffer();
            }

            if (!world.log.HasMessages())
            {
                world.SetState(ctx, RunState.AwaitingInput);
            }
        }
    }
}