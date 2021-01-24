using WasteDrudgers.Level;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    ///<summary>
    /// Base class for scenes that render the game view underneath.
    ///</summary>
    public abstract class GameScene : Scene
    {
        public override void Run(IContext ctx, World world)
        {
            // Queue game layers for rendering, even if they're not rerendered 
            ctx.QueueCanvas(RenderLayer.Root);
            ctx.QueueCanvas(RenderLayer.ViewportASCII)
                .SetRenderPosition(ctx.UIData.viewport.x + 1, ctx.UIData.viewport.y + 1);

            if (world.ShouldRedraw)
            {
                var pos = world.PlayerData.coords;
                HUD.DrawBoxes(ctx);
                HUD.DrawSidebar(ctx, world);
                HUD.DrawStatsBar(ctx, world);
                HUD.DrawStatusBar(ctx, world);
                HUD.DrawFooter(ctx, world, LevelUtils.GetDescription(world, pos));
                HUD.DrawLog(ctx, world);
                HUD.DrawClock(ctx, world, world.Calendar.Hour);
                Viewport.Draw(ctx, world, pos);

                world.ShouldRedraw = false;
            }
            base.Run(ctx, world);
        }
    }
}
