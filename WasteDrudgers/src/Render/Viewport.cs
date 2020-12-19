using System;
using Blaggard;
using Blaggard.Common;
using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers.Render
{
    public static class Viewport
    {
        public static void Draw(IContext ctx, World world, Vec2 origin)
        {
            DrawASCIIMode(ctx, world, origin);
        }

        public static (int x, int y) GetPosScreenCoords(in Vec2 coords, int vx, int vy, int ox, int oy) =>
            (vx + coords.x - ox, vy + coords.y - oy);

        /// <summary>
        /// Draw game viewport in ASCII mode.
        /// This is a bit monstrous, but this is perf-critical code.
        /// </summary>
        public static void DrawASCIIMode(IContext ctx, World world, Vec2 origin)
        {
            var layer = ctx.GetCanvas(RenderLayer.ViewportASCII);
            var map = world.Map;
            var viewport = GetViewportRect(map.width, map.height, layer.Width, layer.Height);

            var (tiles, vis, blood) = (map.GetTiles(), map.GetVisibility(), map.GetBlood());
            var (ox, oy) = CameraOffset(origin, map.width, map.height, viewport.width, viewport.height);

            // Draw tiles
            for (int y = 0; y < viewport.height; y++)
            {
                for (int x = 0; x < viewport.width; x++)
                {
                    var i = Util.IndexFromXY(x + ox, y + oy, map.width);
                    var (tile, v) = (tiles[i], vis[i]);
                    if (v == Visibility.Hidden)
                    {
                        layer.SetCell(x, y, ' ', Data.Colors.black, Data.Colors.black);
                        continue;
                    }
                    var (fore, back) = GetTileColors(tile, v, blood[i]);
                    var ch = (tile.characters.Length > 1 && !ctx.Config.SimpleTiles)
                        ? 1 + HashNoise.GetIndex(i, tile.characters.Length - 1)
                        : 0;

                    layer.SetCell(x, y, tile.characters[ch], fore, back);
                }
            }

            var (positions, renderables, effects) = world.ecs.Pools<Position, Renderable, VisualEffect>();

            // Draw map features
            foreach (var e in world.ecs.View<Position, Renderable, Feature>())
            {
                ref var pos = ref positions[e];
                ref var renderable = ref renderables[e];

                var (x, y) = GetPosScreenCoords(pos.coords, viewport.x, viewport.y, ox, oy);
                if (!viewport.IsWithinBounds(x, y)) continue;

                var i = pos.coords.ToIndex(map.Width);
                switch (vis[i])
                {
                    case Visibility.Explored:
                        layer.SetCell(x, y, renderable.character, Data.Colors.grey, Data.Colors.shadow);
                        break;
                    case Visibility.Visible:
                        layer.PutChar(x, y, renderable.character, renderable.color);
                        break;
                }
            };

            // Draw items
            foreach (var pos in world.spatial.GetPositionsWithItems())
            {
                var (x, y) = GetPosScreenCoords(pos, viewport.x, viewport.y, ox, oy);
                if (!viewport.IsWithinBounds(x, y)) continue;

                var i = pos.ToIndex(map.width);
                if (vis[i] == Visibility.Visible)
                {
                    var renderable = world.spatial.GetItemsRenderable(pos, world);
                    layer.PutChar(x, y, renderable.character, renderable.color);
                }
            }

            // Draw actors
            foreach (var e in world.ecs.View<Position, Renderable, Actor>())
            {
                ref var pos = ref positions[e];
                ref var renderable = ref renderables[e];

                var (x, y) = GetPosScreenCoords(pos.coords, viewport.x, viewport.y, ox, oy);
                if (!viewport.IsWithinBounds(x, y)) continue;

                var i = pos.coords.ToIndex(map.Width);
                if (vis[i] != Visibility.Visible) continue;
                layer.PutChar(x, y, renderable.character, renderable.color);
            };

            // Draw effects
            foreach (var e in world.ecs.View<Position, VisualEffect>())
            {
                ref var pos = ref positions[e];
                ref var eff = ref effects[e];

                var (x, y) = GetPosScreenCoords(pos.coords, viewport.x, viewport.y, ox, oy);
                if (!viewport.IsWithinBounds(x, y)) continue;

                var i = pos.coords.ToIndex(map.width);
                if (vis[i] != Visibility.Visible) continue;

                layer.PutChar(x, y, eff.characters[(int)eff.delta], eff.color);
            };
        }

        public static (Color fore, Color back) GetTileColors(Tile tile, Visibility vis, BloodType blood) =>
            (vis == Visibility.Visible)
                ? (
                    blood switch
                    {
                        BloodType.Red => Data.Colors.redLight,
                        BloodType.Yellow => Data.Colors.beigeLight,
                        _ => tile.foreground
                    },
                    tile.background
                )
                : (Data.Colors.shadowLight, Data.Colors.shadow);

        public static (int xOffset, int yOffset) CameraOffset(Vec2 coords, int mapWidth, int mapHeight, int viewportWidth, int viewportHeight) =>
        (
            Math.Clamp(coords.x - (viewportWidth / 2), 0, Math.Max(0, mapWidth - viewportWidth)),
            Math.Clamp(coords.y - (viewportHeight / 2), 0, Math.Max(0, mapHeight - viewportHeight))
        );


        public static Rect GetViewportRect(int mapWidth, int mapHeight, int fullViewportWidth, int fullViewportHeight)
        {
            var renderOffsetX = Math.Max(0, fullViewportWidth - mapWidth);
            var renderOffsetY = Math.Max(0, fullViewportHeight - mapHeight);

            return new Rect(
                renderOffsetX / 2,
                renderOffsetY / 2,
                fullViewportWidth - renderOffsetX,
                fullViewportHeight - renderOffsetY);
        }
    }
}