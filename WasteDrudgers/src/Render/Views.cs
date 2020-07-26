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

        public static void DrawGameView(IContext ctx, World world)
        {
            // Queue game layers for rendering, even if they're not rerendered 
            (var root, var viewport) = QueueGameLayers(ctx);

            if (world.ShouldRedraw)
            {
                var playerData = world.ecs.FetchResource<PlayerData>();

                HUD.Draw(ctx, root, world, playerData);
                Viewport.Draw(ctx, viewport, ctx.Theme, world, playerData.coords);

                world.ShouldRedraw = false;
            }
        }

        public static void DrawLookView(IContext ctx, World world, Vec2 cursor)
        {
            (var root, var viewport) = QueueGameLayers(ctx);

            if (world.ShouldRedraw)
            {
                (var playerData, var map) = world.ecs.FetchResource<PlayerData, Map>();

                HUD.Draw(ctx, root, world, playerData);
                Viewport.Draw(ctx, viewport, ctx.Theme, world, cursor);

                HUD.DrawFooter(ctx, root, LevelUtils.GetLookDescription(world, cursor));

                var rect = ctx.UIData.viewport.Contract(1);
                Viewport.CameraOffset(cursor, map.width, map.height, rect.width, rect.height, out int xOffset, out int yOffset);
                viewport.PutChar(cursor.x - xOffset, cursor.y - yOffset, 'X', ctx.Theme.white);

                world.ShouldRedraw = false;
            }
        }
    }
}