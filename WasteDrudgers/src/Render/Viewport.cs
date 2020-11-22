using System;
using Blaggard;
using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers.Render
{
    public static class Viewport
    {
        public static void Draw(IContext ctx, IBlittable layer, Theme theme, World world, Vec2 origin, Rect viewport, Map map)
        {
            switch (ctx.Config.Style)
            {
                case GraphicsStyle.Tiles:
                    //DrawSpriteMode(ctx, layer, world, origin);
                    break;

                default:
                    DrawASCIIMode(ctx, layer, world, origin, viewport, map);
                    break;
            }
        }

        public static void DrawASCIIMode(IContext ctx, IBlittable layer, World world, Vec2 origin, Rect viewport, Map map)
        {
            layer.ResetColors();
            layer.Clear();

            var tiles = map.GetTiles();
            var vis = map.GetVisibility();

            CameraOffset(origin, map.width, map.height, viewport.width, viewport.height, out int xOffset, out int yOffset);

            DrawTiles(ctx, layer, viewport, map, xOffset, yOffset);
            DrawEntities<Feature>(ctx, layer, world, viewport, xOffset, yOffset, map.width, vis, false);
            DrawItems(ctx, layer, world, viewport, map, xOffset, yOffset);
            DrawEntities<Actor>(ctx, layer, world, viewport, xOffset, yOffset, map.width, vis);

            foreach (var e in world.ecs.View<Position, Effect>())
            {
                ref var pos = ref world.ecs.GetRef<Position>(e);
                ref var eff = ref world.ecs.GetRef<Effect>(e);

                int x = viewport.x + pos.coords.x - xOffset;
                int y = viewport.y + pos.coords.y - yOffset;
                if (!viewport.IsWithinBounds(x, y)) continue;

                var i = pos.coords.ToIndex(map.width);
                if (vis[i] != Visibility.Visible) continue;

                layer.PutChar(x, y, eff.characters[(int)eff.delta], eff.color);
            };
        }

        public static void CameraOffset(Vec2 coords, int mapWidth, int mapHeight, int viewportWidth, int viewportHeight, out int xOffset, out int yOffset)
        {
            xOffset = Math.Clamp(coords.x - (viewportWidth / 2), 0, Math.Max(0, mapWidth - viewportWidth));
            yOffset = Math.Clamp(coords.y - (viewportHeight / 2), 0, Math.Max(0, mapHeight - viewportHeight));
        }

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

        private static void DrawTiles(IContext ctx, IBlittable layer, Rect viewport, Map map, int xOffset, int yOffset)
        {
            var tiles = map.GetTiles();
            var vis = map.GetVisibility();
            var blood = map.GetBlood();
            for (int y = 0; y < viewport.height; y++)
            {
                for (int x = 0; x < viewport.width; x++)
                {
                    var i = Util.IndexFromXY(x + xOffset, y + yOffset, map.width);
                    if (ctx.Config.Style == GraphicsStyle.Glyphs)
                    {
                        bool wallBelow = i + map.width > map.width * map.height || (tiles[i + map.width].flags & TileFlags.BlocksMovement) > 0;
                        DrawTile(ctx, layer, tiles[i], vis[i], blood[i], x, y, i, wallBelow);
                    }
                    else
                    {
                        DrawTile(ctx, layer, tiles[i], vis[i], blood[i], x + viewport.x, y + viewport.y, i);
                    }
                }
            }
        }

        private static void DrawItems(IContext ctx, IBlittable layer, World world, Rect viewport, Map map, int xOffset, int yOffset)
        {
            var vis = map.GetVisibility();
            foreach (var pos in world.spatial.GetPositionsWithItems())
            {
                int x = viewport.x + pos.x - xOffset;
                int y = viewport.y + pos.y - yOffset;
                if (!viewport.IsWithinBounds(x, y)) continue;

                var i = pos.ToIndex(map.width);
                if (vis[i] == Visibility.Visible)
                {
                    var renderable = world.spatial.GetItemsRenderable(pos, world);
                    char ch = renderable.character;
                    layer.PutChar(x, y, ch, renderable.color);
                }
            }
        }

        private static void DrawTile(IContext ctx, IBlittable layer, Tile tile, Visibility vis, BloodType blood, int x, int y, int i, bool wallBelow = false)
        {
            if (vis == Visibility.Hidden) return;

            switch (ctx.Config.Style)
            {
                default:
                    Color fore, back;
                    if (vis == Visibility.Visible)
                    {
                        fore = blood switch
                        {
                            BloodType.Red => Data.Colors.redLight,
                            BloodType.Yellow => Data.Colors.beigeLight,
                            _ => tile.foreground
                        };
                        back = tile.background;
                    }
                    else
                    {
                        (fore, back) = (Data.Colors.shadowLight, Data.Colors.shadow);
                    }

                    var ch = (tile.characters.Length > 1 && !ctx.Config.SimpleTiles)
                        ? 1 + HashNoise.GetIndex(i, tile.characters.Length - 1)
                        : 0;

                    layer.PutChar(x, y, tile.characters[ch], fore, back);
                    break;
            }
        }

        private static void DrawEntity(IContext ctx, IBlittable layer, int x, int y, Renderable renderable, Visibility vis, bool onlyVisible)
        {
            switch (vis)
            {
                case Visibility.Explored:
                    if (onlyVisible) return;
                    layer.PutChar(x, y, renderable.character, Data.Colors.shadowLight);

                    break;
                case Visibility.Visible:
                    layer.PutChar(x, y, renderable.character, renderable.color);
                    break;
            }
        }

        private static void DrawEntities<T>(IContext ctx, IBlittable layer, World world, Rect viewport, int xOffset, int yOffset, int mapWidth, Visibility[] vis, bool onlyVisible = true) where T : struct
        {
            foreach (var e in world.ecs.View<Position, Renderable, T>())
            {
                ref var pos = ref world.ecs.GetRef<Position>(e);
                ref var renderable = ref world.ecs.GetRef<Renderable>(e);

                int x = viewport.x + pos.coords.x - xOffset;
                int y = viewport.y + pos.coords.y - yOffset;
                if (!viewport.IsWithinBounds(x, y))
                    continue;

                var i = pos.coords.ToIndex(mapWidth);
                DrawEntity(ctx, layer, x, y, renderable, vis[i], onlyVisible);
            };
        }
    }
}