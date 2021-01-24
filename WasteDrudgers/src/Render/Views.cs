using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Level;

namespace WasteDrudgers.Render
{
    public static class Views
    {
        private static (IBlittable root, IBlittable viewport) QueueGameLayers(IContext ctx)
        {
            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.SetRenderPosition(0, 0);

            IBlittable viewport;
            if (ctx.Config.Style != GraphicsStyle.Tiles)
            {
                viewport = ctx.QueueCanvas(RenderLayer.ViewportASCII);
                viewport.SetRenderPosition(ctx.UIData.viewport.x + 1, ctx.UIData.viewport.y + 1);
            }
            else
            {
                viewport = ctx.QueueCanvas(RenderLayer.ViewportTile);
                viewport.SetRenderPosition(ctx.UIData.viewport.x + 1, ctx.UIData.viewport.y + 1);
            }

            return (root, viewport);
        }

        public static void DrawLookView(IContext ctx, World world, Vec2 cursor)
        {
            (var root, var viewport) = QueueGameLayers(ctx);

            if (world.ShouldRedraw)
            {
                var map = world.Map;
                var playerData = world.PlayerData;

                HUD.DrawBoxes(ctx);
                HUD.DrawSidebar(ctx, world);
                HUD.DrawStatsBar(ctx, world);
                HUD.DrawStatusBar(ctx, world);
                HUD.DrawLog(ctx, world);

                Viewport.Draw(ctx, world, cursor);

                HUD.DrawFooter(ctx, world, LevelUtils.GetLookDescription(world, cursor));

                var rect = Viewport.GetViewportRect(map.width, map.height, viewport.Width, viewport.Height);
                var (ox, oy) = Viewport.CameraOffset(cursor, map.width, map.height, rect.width, rect.height);
                viewport.PutChar(cursor.x + rect.x - ox, cursor.y + rect.y - oy, 'X', Data.Colors.white);

                world.ShouldRedraw = false;
            }
        }
    }
}